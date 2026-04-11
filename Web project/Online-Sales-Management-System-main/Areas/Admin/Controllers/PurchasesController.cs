// FILE: OnlineSalesManagementSystem/Areas/Admin/Controllers/PurchasesController.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Purchases + "." + PermissionConstants.Actions.Show)]
public class PurchasesController : Controller
{
    private readonly ApplicationDbContext _db;

    public PurchasesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, string? status, int page = 1, int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Purchases.AsNoTracking()
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(p =>
                p.PurchaseNo.Contains(q) ||
                (p.Supplier != null && p.Supplier.Name.Contains(q)));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PurchaseStatus>(status, true, out var st))
        {
            query = query.Where(p => p.Status == st);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.PurchaseDate)
            .ThenByDescending(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.Status = status;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Purchases + "." + PermissionConstants.Actions.Create)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadLookupsAsync();
        return View(new PurchaseCreateVm
        {
            PurchaseDate = DateTime.UtcNow.Date,
            Items = new List<PurchaseItemVm> { new() } // start with 1 row
        });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Purchases + "." + PermissionConstants.Actions.Create)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PurchaseCreateVm vm)
    {
        await LoadLookupsAsync();

        vm.Items ??= new();
        vm.Items = vm.Items
            .Where(i => i.ProductId.HasValue && i.Qty > 0 && i.UnitCost >= 0)
            .ToList();

        if (vm.SupplierId == null || vm.SupplierId <= 0)
            ModelState.AddModelError(nameof(vm.SupplierId), "Supplier is required.");

        if (vm.PurchaseDate == default)
            ModelState.AddModelError(nameof(vm.PurchaseDate), "Purchase date is required.");

        if (vm.Items.Count == 0)
            ModelState.AddModelError(nameof(vm.Items), "Add at least one item.");

        if (!ModelState.IsValid)
            return View(vm);

        // Load products once for price checks and to avoid repeated DB calls
        var productIds = vm.Items.Select(i => i.ProductId!.Value).Distinct().ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id) && p.IsActive).ToListAsync();

        if (products.Count != productIds.Count)
        {
            ModelState.AddModelError(string.Empty, "One or more selected products are invalid.");
            return View(vm);
        }

        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == vm.SupplierId && s.IsActive);
        if (supplier == null)
        {
            ModelState.AddModelError(nameof(vm.SupplierId), "Supplier not found or inactive.");
            return View(vm);
        }

        var purchase = new Purchase
        {
            PurchaseNo = await GeneratePurchaseNoAsync(),
            SupplierId = supplier.Id,
            PurchaseDate = vm.PurchaseDate.Date,
            Status = PurchaseStatus.Draft
        };

        decimal subTotal = 0m;

        foreach (var row in vm.Items)
        {
            var qty = row.Qty;
            var unitCost = row.UnitCost;

            var lineTotal = qty * unitCost;
            subTotal += lineTotal;

            purchase.Items.Add(new PurchaseItem
            {
                ProductId = row.ProductId!.Value,
                Qty = qty,
                UnitCost = unitCost,
                LineTotal = lineTotal
            });
        }

        purchase.SubTotal = subTotal;
        purchase.GrandTotal = subTotal;

        _db.Purchases.Add(purchase);
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = $"Purchase created: {purchase.PurchaseNo}";
        return RedirectToAction(nameof(Details), new { id = purchase.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var purchase = await _db.Purchases.AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (purchase == null) return NotFound();
        return View(purchase);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Purchases + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Receive(int id)
    {
        var purchase = await _db.Purchases
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (purchase == null) return NotFound();

        if (purchase.Status != PurchaseStatus.Draft)
        {
            TempData["ToastError"] = "Only Draft purchases can be received.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Load all products in one go
        var productIds = purchase.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products.Where(p => productIds.Contains(p.Id)).ToListAsync();

        var now = DateTime.UtcNow;

        foreach (var item in purchase.Items)
        {
            var prod = products.First(p => p.Id == item.ProductId);
            prod.StockOnHand += item.Qty;

            _db.StockMovements.Add(new StockMovement
            {
                ProductId = prod.Id,
                MovementDate = now,
                Type = StockMovementType.In,
                Qty = item.Qty,
                RefType = "Purchase",
                RefId = purchase.Id,
                Note = $"Receive {purchase.PurchaseNo}"
            });
        }

        purchase.Status = PurchaseStatus.Received;

        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Purchase received. Stock updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Purchases + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var purchase = await _db.Purchases.FirstOrDefaultAsync(p => p.Id == id);
        if (purchase == null) return NotFound();

        if (purchase.Status == PurchaseStatus.Received)
        {
            TempData["ToastError"] = "Cannot cancel a Received purchase (stock has already been updated).";
            return RedirectToAction(nameof(Details), new { id });
        }

        purchase.Status = PurchaseStatus.Cancelled;
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Purchase cancelled.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task LoadLookupsAsync()
    {
        ViewBag.Suppliers = await _db.Suppliers.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

        ViewBag.Products = await _db.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    private async Task<string> GeneratePurchaseNoAsync()
    {
        // Unique enough for demo/lab: PO-YYYYMMDD-HHMMSS-XXXX
        for (int i = 0; i < 5; i++)
        {
            var stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            var rand = Random.Shared.Next(1000, 9999);
            var no = $"PO-{stamp}-{rand}";

            var exists = await _db.Purchases.AnyAsync(p => p.PurchaseNo == no);
            if (!exists) return no;

            await Task.Delay(20);
        }

        // Fallback
        return $"PO-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}";
    }

    // ===== ViewModels for Create =====
    public sealed class PurchaseCreateVm
    {
        [Required]
        public int? SupplierId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; }

        public List<PurchaseItemVm> Items { get; set; } = new();
    }

    public sealed class PurchaseItemVm
    {
        [Required]
        public int? ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Qty { get; set; } = 1;

        [Range(0, double.MaxValue)]
        public decimal UnitCost { get; set; } = 0m;
    }
}
 
