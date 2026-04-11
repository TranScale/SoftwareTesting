// FILE: OnlineSalesManagementSystem/Areas/Admin/Controllers/SuppliersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Suppliers + "." + PermissionConstants.Actions.Show)]
public class SuppliersController : Controller
{
    private readonly ApplicationDbContext _db;

    public SuppliersController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Suppliers
            .AsNoTracking()
            .Where(s => s.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(s =>
                s.Name.Contains(q) ||
                (s.Phone != null && s.Phone.Contains(q)) ||
                (s.Email != null && s.Email.Contains(q)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .ThenByDescending(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Suppliers + "." + PermissionConstants.Actions.Create)]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Supplier { IsActive = true, CreatedAt = DateTime.UtcNow });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Suppliers + "." + PermissionConstants.Actions.Create)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Supplier model)
    {
        model.Name = (model.Name ?? "").Trim();
        model.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        model.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();

        if (!ModelState.IsValid)
            return View(model);

        model.IsActive = true;
        model.CreatedAt = DateTime.UtcNow;

        _db.Suppliers.Add(model);
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Supplier created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Suppliers + "." + PermissionConstants.Actions.Edit)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
        if (entity == null) return NotFound();

        return View(entity);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Suppliers + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Supplier model)
    {
        model.Name = (model.Name ?? "").Trim();
        model.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        model.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();

        if (!ModelState.IsValid)
            return View(model);

        var entity = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == model.Id && s.IsActive);
        if (entity == null) return NotFound();

        entity.Name = model.Name;
        entity.Phone = model.Phone;
        entity.Email = model.Email;
        entity.Address = model.Address;

        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Supplier updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Suppliers + "." + PermissionConstants.Actions.Delete)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
        if (entity == null) return NotFound();

        entity.IsActive = false;
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Supplier deleted (disabled).";
        return RedirectToAction(nameof(Index));
    }

    // --- UPDATED: DETAILS ACTION TO FETCH HISTORY ---
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        // 1. Lấy thông tin Nhà cung cấp
        var supplier = await _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (supplier == null) return NotFound();

        // 2. Lấy 20 giao dịch nhập hàng gần nhất
        var history = await _db.Purchases
            .AsNoTracking()
            .Where(p => p.SupplierId == id)
            .OrderByDescending(p => p.PurchaseDate)
            .Take(20)
            .Select(p => new SupplierPurchaseHistoryRow
            {
                PurchaseId = p.Id,
                PurchaseDate = p.PurchaseDate,
                PurchaseNo = p.PurchaseNo,
                // Tính tổng tiền: Tổng số lượng * Đơn giá nhập của từng sản phẩm trong phiếu
                TotalAmount = p.Items.Sum(i => i.Qty * i.UnitCost),
                Status = p.Status
            })
            .ToListAsync();

        // 3. Đóng gói vào ViewModel
        var vm = new SupplierDetailsVm
        {
            Supplier = supplier,
            History = history
        };

        return View(vm);
    }

    // ====== VIEWMODELS ======
    public class SupplierDetailsVm
    {
        public Supplier Supplier { get; set; } = default!;
        public List<SupplierPurchaseHistoryRow> History { get; set; } = new();
    }

    public class SupplierPurchaseHistoryRow
    {
        public int PurchaseId { get; set; }
        public string PurchaseNo { get; set; } = "";
        public DateTime PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
        public PurchaseStatus Status { get; set; }
    }
}