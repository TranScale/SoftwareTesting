using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Expenses + "." + PermissionConstants.Actions.Show)]
public class ExpensesController : Controller
{
    private readonly ApplicationDbContext _db;

    public ExpensesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? from, DateTime? to, int page = 1, int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Expenses.AsNoTracking().Where(e => e.IsActive).AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.ExpenseDate >= from.Value.Date);

        if (to.HasValue)
            query = query.Where(e => e.ExpenseDate <= to.Value.Date);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.ExpenseDate)
            .ThenByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.From = from?.Date.ToString("yyyy-MM-dd");
        ViewBag.To = to?.Date.ToString("yyyy-MM-dd");
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;

        return View(items);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Expenses + "." + PermissionConstants.Actions.Create)]
    [HttpGet]
    public IActionResult Create()
    {
        return View(new Expense { ExpenseDate = DateTime.UtcNow.Date, Amount = 0m });
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Expenses + "." + PermissionConstants.Actions.Create)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Expense model)
    {
        model.Title = (model.Title ?? "").Trim();
        model.Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim();

        if (!ModelState.IsValid)
            return View(model);

        if (model.ExpenseDate == default)
            model.ExpenseDate = DateTime.UtcNow.Date;

        model.IsActive = true;
        _db.Expenses.Add(model);
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Expense created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Expenses + "." + PermissionConstants.Actions.Edit)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
        if (entity == null) return NotFound();

        return View(entity);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Expenses + "." + PermissionConstants.Actions.Edit)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Expense model)
    {
        model.Title = (model.Title ?? "").Trim();
        model.Note = string.IsNullOrWhiteSpace(model.Note) ? null : model.Note.Trim();

        if (!ModelState.IsValid)
            return View(model);

        var entity = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == model.Id && e.IsActive);
        if (entity == null) return NotFound();

        entity.Title = model.Title;
        entity.Amount = model.Amount;
        entity.ExpenseDate = model.ExpenseDate == default ? entity.ExpenseDate : model.ExpenseDate.Date;
        entity.Note = model.Note;

        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Expense updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Expenses + "." + PermissionConstants.Actions.Delete)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id && e.IsActive);
        if (entity == null) return NotFound();

        entity.IsActive = false;
        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Expense deleted (disabled).";
        return RedirectToAction(nameof(Index));
    }
}
 