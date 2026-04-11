using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using OnlineSalesManagementSystem.Services.Security;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Brands + "." + PermissionConstants.Actions.Show)]
public class BrandsController : Controller
{
    private readonly ApplicationDbContext _db;

    public BrandsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Brands
            .AsNoTracking()
            .Where(b => b.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(b => b.Name.Contains(q));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(b => b.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Brands + "." + PermissionConstants.Actions.Create)]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Brand { IsActive = true });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Brands + "." + PermissionConstants.Actions.Create)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Brand model)
    {
        model.Name = (model.Name ?? string.Empty).Trim();

        if (!ModelState.IsValid)
            return View(model);

        var exists = await _db.Brands.AnyAsync(b => b.Name == model.Name && b.IsActive);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "Brand name already exists.");
            return View(model);
        }

        model.IsActive = true;
        _db.Brands.Add(model);
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Brand created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Brands + "." + PermissionConstants.Actions.Edit)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _db.Brands.FirstOrDefaultAsync(b => b.Id == id && b.IsActive);
        if (entity == null) return NotFound();

        return View(entity);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Brands + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Brand model)
    {
        model.Name = (model.Name ?? string.Empty).Trim();

        if (!ModelState.IsValid)
            return View(model);

        var entity = await _db.Brands.FirstOrDefaultAsync(b => b.Id == model.Id && b.IsActive);
        if (entity == null) return NotFound();

        var exists = await _db.Brands.AnyAsync(b => b.Id != model.Id && b.Name == model.Name && b.IsActive);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "Brand name already exists.");
            return View(model);
        }

        entity.Name = model.Name;
        entity.IsActive = model.IsActive;

        _db.Brands.Update(entity);
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Brand updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Brands + "." + PermissionConstants.Actions.Delete)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Brands.FirstOrDefaultAsync(b => b.Id == id && b.IsActive);
        if (entity == null) return NotFound();

        // Soft delete để tránh vỡ FK nếu sản phẩm đang trỏ tới brand.
        entity.IsActive = false;
        _db.Brands.Update(entity);
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Brand deleted.";
        return RedirectToAction(nameof(Index));
    }
}
