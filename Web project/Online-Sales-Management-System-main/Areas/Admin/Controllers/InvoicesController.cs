using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Services.Common;
using System.ComponentModel.DataAnnotations;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Invoices + "." + PermissionConstants.Actions.Show)]
public class InvoicesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<InvoicesController> _logger;

    public InvoicesController(ApplicationDbContext db, ILogger<InvoicesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ========= INDEX =========
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
    {
        q ??= "";
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .OrderByDescending(i => i.Id)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var qq = q.Trim().ToLower();
            query = query.Where(i =>
                i.InvoiceNo.ToLower().Contains(qq) ||
                (i.Customer != null && i.Customer.Name.ToLower().Contains(qq)));
        }

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    // ========= DETAILS / PRINT =========
    public async Task<IActionResult> Details(int id)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Items).ThenInclude(it => it.Product)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return NotFound();
        return View(invoice);
    }

    public async Task<IActionResult> Print(int id)
    {
        var invoice = await _db.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.Items).ThenInclude(it => it.Product)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return NotFound();
        return View(invoice);
    }

    // ========= CREATE =========
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Invoices + "." + PermissionConstants.Actions.Create)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadLookupsAsync();
        return View(new InvoiceCreateVm
        {
            InvoiceDate = AppTime.VietnamNow(),
            Items = new List<InvoiceItemVm> { new() },
            PaidAmount = 0m
        });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Invoices + "." + PermissionConstants.Actions.Create)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InvoiceCreateVm vm)
    {
        var items = vm.Items ?? new List<InvoiceItemVm>();

        if (items.Count == 0)
        {
            ModelState.AddModelError("", "Please add at least 1 item.");
        }
        else if (items.Any(x => x.ProductId == null))
        {
            ModelState.AddModelError("", "Please select product for all items.");
        }

        if (vm.CustomerId.HasValue &&
            !await _db.Customers.AsNoTracking().AnyAsync(c => c.Id == vm.CustomerId.Value && c.IsActive))
        {
            ModelState.AddModelError(nameof(vm.CustomerId), "Selected customer is invalid.");
        }

        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync();
            return View(vm);
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        try
        {
            var customer = vm.CustomerId.HasValue
                ? await _db.Customers.FirstOrDefaultAsync(c => c.Id == vm.CustomerId.Value)
                : null;

            var invoice = new Invoice
            {
                InvoiceNo = await GenerateInvoiceNoAsync(),
                CustomerId = customer?.Id,
                // Store UTC in DB but respect the Vietnam-local time shown on the form.
                InvoiceDate = vm.InvoiceDate == default ? AppTime.UtcNow() : AppTime.VietnamToUtc(vm.InvoiceDate),
                PaidAmount = vm.PaidAmount
            };

            decimal subTotal = 0m;

            // Always use server-side SalePrice (do NOT trust posted UnitPrice)
            var productIds = items.Where(x => x.ProductId.HasValue).Select(x => x.ProductId!.Value).Distinct().ToList();
            var priceMap = await _db.Products
                .AsNoTracking()
                .Where(p => p.IsActive && productIds.Contains(p.Id))
                .Select(p => new { p.Id, p.SalePrice, p.Name })
                .ToDictionaryAsync(x => x.Id, x => x);

            foreach (var row in items)
            {
                var qty = row.Qty;
                if (qty < 1) qty = 1;

                if (!row.ProductId.HasValue || !priceMap.TryGetValue(row.ProductId.Value, out var pinfo))
                    throw new InvalidOperationException("Invalid product selected.");

                var price = pinfo.SalePrice;
                var lineTotal = qty * price;
                subTotal += lineTotal;

                invoice.Items.Add(new InvoiceItem
                {
                    ProductId = row.ProductId!.Value,
                    Quantity = qty,
                    UnitPrice = price,
                    LineTotal = lineTotal
                });
            }

            invoice.SubTotal = subTotal;
            invoice.GrandTotal = subTotal;

            // normalize payment + status
            if (invoice.GrandTotal <= 0)
            {
                invoice.PaidAmount = 0;
                invoice.Status = InvoiceStatus.Paid;
            }
            else if (invoice.PaidAmount >= invoice.GrandTotal)
            {
                invoice.PaidAmount = invoice.GrandTotal;
                invoice.Status = InvoiceStatus.Paid;
            }
            else if (invoice.PaidAmount > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }
            else
            {
                invoice.Status = InvoiceStatus.Unpaid;
            }

            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Reduce stock atomically to avoid oversell race conditions.
            // We update: StockOnHand = StockOnHand - qty WHERE StockOnHand >= qty
            foreach (var it in invoice.Items)
            {
                var productName = priceMap.TryGetValue(it.ProductId, out var p) ? p.Name : $"#{it.ProductId}";

                var affected = await _db.Database.ExecuteSqlInterpolatedAsync($@"
UPDATE Products
SET StockOnHand = StockOnHand - {it.Quantity}
WHERE Id = {it.ProductId} AND StockOnHand >= {it.Quantity}");

                if (affected <= 0)
                    throw new InvalidOperationException($"Not enough stock for '{productName}'. Please refresh and try again.");

                _db.StockMovements.Add(new StockMovement
                {
                    ProductId = it.ProductId,
                    MovementDate = AppTime.UtcNow(),
                    Type = StockMovementType.Out,
                    Qty = it.Quantity,
                    RefType = "Invoice",
                    RefId = invoice.Id,
                    Note = invoice.InvoiceNo
                });
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["ToastSuccess"] = "Invoice created successfully.";
            return RedirectToAction(nameof(Details), new { id = invoice.Id });
        }
        catch (Exception ex)
        {
            try
            {
                await tx.RollbackAsync();
            }
            catch (Exception rollbackEx)
            {
                _logger.LogWarning(rollbackEx, "Rollback failed while creating invoice.");
            }

            _logger.LogError(ex, "Failed to create invoice.");
            TempData["ToastError"] = "Failed to create invoice. Please check data and try again.";

            await LoadLookupsAsync();
            return View(vm);
        }
    }

    // ========= RECORD PAYMENT =========
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Invoices + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordPayment(int id, decimal amount)
    {
        if (amount <= 0)
        {
            TempData["ToastError"] = "Amount must be greater than 0.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == id);
        if (invoice == null) return NotFound();
        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            TempData["ToastError"] = "Cancelled invoice cannot be paid.";
            return RedirectToAction(nameof(Details), new { id });
        }

        invoice.PaidAmount += amount;

        if (invoice.GrandTotal <= 0)
        {
            invoice.Status = InvoiceStatus.Paid;
        }
        else if (invoice.PaidAmount >= invoice.GrandTotal)
        {
            invoice.PaidAmount = invoice.GrandTotal;
            invoice.Status = InvoiceStatus.Paid;
        }
        else if (invoice.PaidAmount > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }
        else
        {
            invoice.Status = InvoiceStatus.Unpaid;
        }

        await _db.SaveChangesAsync();
        TempData["ToastSuccess"] = "Payment recorded.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ========= CANCEL =========
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Invoices + "." + PermissionConstants.Actions.Delete)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null) return NotFound();
        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            TempData["ToastInfo"] = "Invoice already cancelled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var productIds = invoice.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var it in invoice.Items)
            {
                if (!products.TryGetValue(it.ProductId, out var product))
                {
                    throw new InvalidOperationException($"Product #{it.ProductId} not found.");
                }

                product.StockOnHand += it.Quantity;

                _db.StockMovements.Add(new StockMovement
                {
                    ProductId = it.ProductId,
                    MovementDate = AppTime.UtcNow(),
                    Type = StockMovementType.In,
                    Qty = it.Quantity,
                    RefType = "InvoiceCancel",
                    RefId = invoice.Id,
                    Note = invoice.InvoiceNo
                });
            }

            invoice.Status = InvoiceStatus.Cancelled;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            TempData["ToastSuccess"] = "Invoice cancelled and stock returned.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            _logger.LogError(ex, "Failed to cancel invoice {InvoiceId}.", id);
            TempData["ToastError"] = "Failed to cancel invoice. Please try again.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    // ========= HELPERS =========
    private async Task LoadLookupsAsync()
    {
        ViewBag.Customers = await _db.Customers.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Products = await _db.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    private Task<string> GenerateInvoiceNoAsync()
        => Task.FromResult($"INV-{Guid.NewGuid().ToString("N")[..12].ToUpperInvariant()}");

    // ===== ViewModels for Create =====
    public sealed class InvoiceCreateVm
    {
        public int? CustomerId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        [Range(0, double.MaxValue)]
        public decimal PaidAmount { get; set; } = 0m;

        public List<InvoiceItemVm> Items { get; set; } = new();
    }

    public sealed class InvoiceItemVm
    {
        [Required]
        public int? ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Qty { get; set; } = 1;

        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; } = 0m;
    }
}
 
 
