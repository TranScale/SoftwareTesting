// FILE: OnlineSalesManagementSystem/Areas/Admin/Controllers/CustomersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using OnlineSalesManagementSystem.Areas.Admin.ViewModels.Customers;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Customers + "." + PermissionConstants.Actions.Show)]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _db;

    public CustomersController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Customers.AsNoTracking()
            .Where(c => c.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(c =>
                c.Name.Contains(q) ||
                (c.Phone != null && c.Phone.Contains(q)) ||
                (c.Email != null && c.Email.Contains(q)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .ThenByDescending(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Customers + "." + PermissionConstants.Actions.Create)]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Customer { IsActive = true, CreatedAt = DateTime.UtcNow });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Customers + "." + PermissionConstants.Actions.Create)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer model)
    {
        model.Name = (model.Name ?? "").Trim();
        model.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        model.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();

        if (!ModelState.IsValid)
            return View(model);

        model.IsActive = true;
        model.CreatedAt = DateTime.UtcNow;

        _db.Customers.Add(model);
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Customer created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Customers + "." + PermissionConstants.Actions.Edit)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        if (entity == null) return NotFound();

        return View(entity);
    }


    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var customer = await _db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (customer == null) return NotFound();

        var invoices = await _db.Invoices.AsNoTracking()
            .Where(i => i.CustomerId == id)
            .Include(i => i.Items)
                .ThenInclude(it => it.Product)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

        var orders = invoices.Select(i => new CustomerOrderRowVm
        {
            Id = i.Id,
            InvoiceNo = i.InvoiceNo,
            InvoiceDate = i.InvoiceDate,
            Status = i.Status,
            SubTotal = i.SubTotal,
            GrandTotal = i.GrandTotal,
            PaidAmount = i.PaidAmount,
            Balance = Math.Max(0, i.GrandTotal - i.PaidAmount),
            ItemCount = i.Items?.Sum(x => x.Quantity) ?? 0,
            ProductPreview = i.Items == null
                ? string.Empty
                : string.Join(", ", i.Items
                    .Take(3)
                    .Select(x => x.Product != null ? x.Product.Name : $"#{x.ProductId}"))
        }).ToList();

        var vm = new CustomerDetailsVm
        {
            Customer = customer,
            Orders = orders,
            TotalOrders = orders.Count,
            TotalSpent = invoices.Where(x => x.Status != InvoiceStatus.Cancelled).Sum(x => x.GrandTotal),
            TotalPaid = invoices.Sum(x => x.PaidAmount),
            TotalOutstanding = invoices.Where(x => x.Status != InvoiceStatus.Cancelled).Sum(x => Math.Max(0, x.GrandTotal - x.PaidAmount)),
            LastOrder = orders.FirstOrDefault()
        };

        return View(vm);
    }
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Customers + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Customer model)
    {
        model.Name = (model.Name ?? "").Trim();
        model.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        model.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();

        if (!ModelState.IsValid)
            return View(model);

        var entity = await _db.Customers.FirstOrDefaultAsync(c => c.Id == model.Id && c.IsActive);
        if (entity == null) return NotFound();

        entity.Name = model.Name;
        entity.Phone = model.Phone;
        entity.Email = model.Email;
        entity.Address = model.Address;

        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Customer updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Customers + "." + PermissionConstants.Actions.Delete)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (entity == null) return NotFound();

        entity.IsActive = false;
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Customer deleted (disabled).";
        return RedirectToAction(nameof(Index));
    }


    // GET: /Admin/Customers/QuickInfo/5
    // Dùng cho tooltip/popup nhanh ở trang Hóa đơn & Tạo hóa đơn
    [HttpGet]
    public async Task<IActionResult> QuickInfo(int id)
    {
        var c = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (c == null) return Json(new { success = false });

        // Tổng đơn + tổng chi (không tính đơn hủy)
        var invoices = await _db.Invoices.AsNoTracking()
            .Where(x => x.CustomerId == id)
            .ToListAsync();

        var totalOrders = invoices.Count;
        var totalSpent = invoices
            .Where(x => x.Status != InvoiceStatus.Cancelled)
            .Sum(x => x.GrandTotal);

        return Json(new
        {
            success = true,
            id = c.Id,
            name = c.Name,
            phone = c.Phone,
            email = c.Email,
            address = c.Address,
            totalOrders,
            totalSpent,
            totalSpentText = totalSpent.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + " VNĐ"
        });
    }

}
