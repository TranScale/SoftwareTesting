using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Employees + "." + PermissionConstants.Actions.Show)]
public class EmployeesController : Controller
{
    private readonly ApplicationDbContext _db;

    public EmployeesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Employees.AsNoTracking()
            .Where(e => e.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(e =>
                e.Name.Contains(q) ||
                (e.Phone != null && e.Phone.Contains(q)) ||
                (e.Email != null && e.Email.Contains(q)) ||
                (e.Position != null && e.Position.Contains(q)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .ThenByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Employees + "." + PermissionConstants.Actions.Create)]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Employee
        {
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Salary = 0m
        });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Employees + "." + PermissionConstants.Actions.Create)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Employee model)
    {
        Normalize(model);

        if (!ModelState.IsValid)
            return View(model);

        model.IsActive = true;
        model.CreatedAt = DateTime.UtcNow;

        _db.Employees.Add(model);
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Employee created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Employees + "." + PermissionConstants.Actions.Edit)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
        if (entity == null) return NotFound();

        return View(entity);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Employees + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Employee model)
    {
        Normalize(model);

        if (!ModelState.IsValid)
            return View(model);

        var entity = await _db.Employees.FirstOrDefaultAsync(e => e.Id == model.Id && e.IsActive);
        if (entity == null) return NotFound();

        entity.Name = model.Name;
        entity.Phone = model.Phone;
        entity.Email = model.Email;
        entity.Address = model.Address;
        entity.Position = model.Position;
        entity.Salary = model.Salary;

        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Employee updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Employees + "." + PermissionConstants.Actions.Delete)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null) return NotFound();

        entity.IsActive = false;
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Employee deleted (disabled).";
        return RedirectToAction(nameof(Index));
    }

    private static void Normalize(Employee model)
    {
        model.Name = (model.Name ?? "").Trim();
        model.Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim();
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
        model.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();
        model.Position = string.IsNullOrWhiteSpace(model.Position) ? null : model.Position.Trim();
        if (model.Salary < 0) model.Salary = 0;
    }
}
