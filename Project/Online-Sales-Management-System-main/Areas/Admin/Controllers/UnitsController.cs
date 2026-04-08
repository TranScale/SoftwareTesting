// FILE: OnlineSalesManagementSystem/Areas/Admin/Controllers/UnitsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Units + "." + PermissionConstants.Actions.Show)]
public class UnitsController : Controller
{
    private readonly ApplicationDbContext _db;

    public UnitsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Units.AsNoTracking().Where(u => u.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(u => u.Name.Contains(q) || (u.ShortName != null && u.ShortName.Contains(q)));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Units + "." + PermissionConstants.Actions.Create)]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Unit());
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Units + "." + PermissionConstants.Actions.Create)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Unit model)
    {
        model.Name = (model.Name ?? "").Trim();
        model.ShortName = string.IsNullOrWhiteSpace(model.ShortName) ? null : model.ShortName.Trim();

        if (!ModelState.IsValid)
            return View(model);

        var existing = await _db.Units.FirstOrDefaultAsync(u => u.Name == model.Name);
        if (existing != null)
        {
            if (existing.IsActive)
            {
                ModelState.AddModelError(nameof(model.Name), "Unit name already exists.");
                return View(model);
            }

            // Nếu unit từng bị xoá (IsActive=false) thì khôi phục lại
            existing.IsActive = true;
            existing.ShortName = model.ShortName;
            await _db.SaveChangesAsync();

            TempData["ToastSuccess"] = "Unit restored.";
            return RedirectToAction(nameof(Index));
        }

        model.IsActive = true;
        _db.Units.Add(model);
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Unit created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Units + "." + PermissionConstants.Actions.Edit)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _db.Units.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        if (entity == null) return NotFound();

        return View(entity);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Units + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Unit model)
    {
        model.Name = (model.Name ?? "").Trim();
        model.ShortName = string.IsNullOrWhiteSpace(model.ShortName) ? null : model.ShortName.Trim();

        if (!ModelState.IsValid)
            return View(model);

        var entity = await _db.Units.FirstOrDefaultAsync(u => u.Id == model.Id && u.IsActive);
        if (entity == null) return NotFound();

        var exists = await _db.Units.AnyAsync(u => u.Id != model.Id && u.Name == model.Name);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "Unit name already exists.");
            return View(model);
        }

        entity.Name = model.Name;
        entity.ShortName = model.ShortName;

        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Unit updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Units + "." + PermissionConstants.Actions.Delete)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Units.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        if (entity == null) return NotFound();

        var used = await _db.Products.AnyAsync(p => p.UnitId == id);
        if (used)
        {
            TempData["ToastError"] = "Cannot delete: this unit is used by one or more products.";
            return RedirectToAction(nameof(Index));
        }

        entity.IsActive = false;
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Unit deleted (disabled).";
        return RedirectToAction(nameof(Index));
    }
}
 