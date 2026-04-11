using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;
using OnlineSalesManagementSystem.Services.Security;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/Products")]
public sealed class ProductWriteController : Controller
{
    private readonly ApplicationDbContext _db;

    public ProductWriteController(ApplicationDbContext db)
    {
        _db = db;
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Edit)]
    [HttpPost("Edit")]
    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Product model)
    {
        model.SKU = (model.SKU ?? string.Empty).Trim();
        model.Name = (model.Name ?? string.Empty).Trim();
        model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        model.ImagePath = string.IsNullOrWhiteSpace(model.ImagePath) ? null : model.ImagePath.Trim();

        if (model.CategoryId.HasValue && model.CategoryId.Value == 0) model.CategoryId = null;
        if (model.UnitId.HasValue && model.UnitId.Value == 0) model.UnitId = null;
        if (model.BrandId.HasValue && model.BrandId.Value == 0) model.BrandId = null;

        await LoadLookupsAsync();

        if (string.IsNullOrWhiteSpace(model.SKU)) ModelState.AddModelError(nameof(model.SKU), "SKU is required.");
        if (string.IsNullOrWhiteSpace(model.Name)) ModelState.AddModelError(nameof(model.Name), "Name is required.");
        if (model.CostPrice < 0) ModelState.AddModelError(nameof(model.CostPrice), "Cost price cannot be negative.");
        if (model.SalePrice < 0) ModelState.AddModelError(nameof(model.SalePrice), "Sale price cannot be negative.");
        if (model.ReorderLevel < 0) ModelState.AddModelError(nameof(model.ReorderLevel), "Reorder level cannot be negative.");

        if (model.CategoryId.HasValue &&
            !await _db.Categories.AsNoTracking().AnyAsync(c => c.Id == model.CategoryId.Value && c.IsActive))
        {
            ModelState.AddModelError(nameof(model.CategoryId), "Selected category is invalid.");
        }

        if (model.UnitId.HasValue &&
            !await _db.Units.AsNoTracking().AnyAsync(u => u.Id == model.UnitId.Value && u.IsActive))
        {
            ModelState.AddModelError(nameof(model.UnitId), "Selected unit is invalid.");
        }

        if (model.BrandId.HasValue &&
            !await _db.Brands.AsNoTracking().AnyAsync(b => b.Id == model.BrandId.Value && b.IsActive))
        {
            ModelState.AddModelError(nameof(model.BrandId), "Selected brand is invalid.");
        }

        var duplicateSku = await _db.Products.AnyAsync(p => p.Id != model.Id && p.SKU == model.SKU && p.IsActive);
        if (duplicateSku)
            ModelState.AddModelError(nameof(model.SKU), "SKU already exists.");

        if (!ModelState.IsValid)
            return View("~/Areas/Admin/Views/Products/Edit.cshtml", model);

        var existing = await _db.Products.FindAsync(model.Id);
        if (existing == null) return NotFound();

        existing.SKU = model.SKU;
        existing.Name = model.Name;
        existing.CategoryId = model.CategoryId;
        existing.UnitId = model.UnitId;
        existing.BrandId = model.BrandId;
        existing.Description = model.Description;
        existing.SalePrice = model.SalePrice;
        existing.CostPrice = model.CostPrice;
        existing.ReorderLevel = model.ReorderLevel;
        existing.ImagePath = model.ImagePath;
        existing.IsTrending = model.IsTrending;
        existing.IsActive = model.IsActive;

        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Product updated.";
        return RedirectToAction("Index", "Products", new { area = "Admin" });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Products + "." + PermissionConstants.Actions.Delete)]
    [HttpPost("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (entity == null) return NotFound();

        if (!entity.IsActive)
        {
            TempData["ToastInfo"] = "Product already inactive.";
            return RedirectToAction("Index", "Products", new { area = "Admin" });
        }

        entity.IsActive = false;
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Product deleted (disabled).";
        return RedirectToAction("Index", "Products", new { area = "Admin" });
    }

    private async Task LoadLookupsAsync()
    {
        ViewBag.Categories = await _db.Categories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();
        ViewBag.Units = await _db.Units.AsNoTracking().Where(u => u.IsActive).OrderBy(u => u.Name).ToListAsync();
        ViewBag.Brands = await _db.Brands.AsNoTracking().Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync();
    }
}
