using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Categories + "." + PermissionConstants.Actions.Show)]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _db;

    public CategoriesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive) // Vẫn giữ logic chỉ hiện Active
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(c => c.Name.Contains(q));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.IsTrending) // Ưu tiên hiện Trending lên đầu
            .ThenBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Categories + "." + PermissionConstants.Actions.Create)]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Category { IsActive = true, IsTrending = false });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Categories + "." + PermissionConstants.Actions.Create)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category model)
    {
        model.Name = (model.Name ?? "").Trim();
        model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();

        if (!ModelState.IsValid)
            return View(model);

        var exists = await _db.Categories.AnyAsync(c => c.Name == model.Name);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "Category name already exists.");
            return View(model);
        }

        model.IsActive = true;
        // model.IsTrending tự động bind từ checkbox

        _db.Categories.Add(model);
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Category created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Categories + "." + PermissionConstants.Actions.Edit)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        if (entity == null) return NotFound();

        return View(entity);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Categories + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Category model)
    {
        model.Name = (model.Name ?? "").Trim();
        model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();

        if (!ModelState.IsValid)
            return View(model);

        var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == model.Id && c.IsActive);
        if (entity == null) return NotFound();

        var exists = await _db.Categories.AnyAsync(c => c.Id != model.Id && c.Name == model.Name);
        if (exists)
        {
            ModelState.AddModelError(nameof(model.Name), "Category name already exists.");
            return View(model);
        }

        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.IsTrending = model.IsTrending; // <--- Update Trending

        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Category updated.";
        return RedirectToAction(nameof(Index));
    }

    // --- MỚI THÊM: Action Toggle nhanh ---
    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Categories + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTrending(int id)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        if (entity == null) return Json(new { success = false, message = "Not found" });

        entity.IsTrending = !entity.IsTrending;
        await _db.SaveChangesAsync();

        return Json(new { success = true, isTrending = entity.IsTrending });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Categories + "." + PermissionConstants.Actions.Delete)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (entity == null) return NotFound();

        var used = await _db.Products.AnyAsync(p => p.CategoryId == id);
        if (used)
        {
            TempData["ToastError"] = "Cannot delete: this category is used by one or more products.";
            return RedirectToAction(nameof(Index));
        }

        entity.IsActive = false;
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Category deleted (disabled).";
        return RedirectToAction(nameof(Index));
    }
}
