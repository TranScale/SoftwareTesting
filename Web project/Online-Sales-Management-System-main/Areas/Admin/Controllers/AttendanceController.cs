using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Services.Security;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Attendance + "." + PermissionConstants.Actions.Show)]
public class AttendanceController : Controller
{
    private readonly ApplicationDbContext _db;

    public AttendanceController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? from, DateTime? to)
    {
        var query = _db.Attendances
            .AsNoTracking()
            .Include(a => a.Employee)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(a => a.Date >= from.Value.Date);

        if (to.HasValue)
            query = query.Where(a => a.Date <= to.Value.Date);

        var items = await query
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.Employee != null ? a.Employee.Name : "")
            .Take(500)
            .ToListAsync();

        ViewBag.From = from?.Date.ToString("yyyy-MM-dd");
        ViewBag.To = to?.Date.ToString("yyyy-MM-dd");

        return View(items);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Attendance + "." + PermissionConstants.Actions.Create)]
    [HttpGet]
    public async Task<IActionResult> Mark(DateTime? date)
    {
        var d = (date ?? DateTime.UtcNow.Date).Date;

        var employees = await _db.Employees.AsNoTracking().Where(e => e.IsActive).OrderBy(e => e.Name).ToListAsync();
        var existing = await _db.Attendances.AsNoTracking().Where(a => a.Date == d).ToListAsync();

        var vm = new AttendanceMarkVm
        {
            Date = d,
            Rows = employees.Select(e =>
            {
                var found = existing.FirstOrDefault(x => x.EmployeeId == e.Id);
                return new AttendanceMarkRowVm
                {
                    EmployeeId = e.Id,
                    EmployeeName = e.Name,
                    Status = found?.Status ?? AttendanceStatus.Present,
                    Note = found?.Note
                };
            }).ToList()
        };

        return View(vm);
    }

    [Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Attendance + "." + PermissionConstants.Actions.Create)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Mark(AttendanceMarkVm vm)
    {
        var d = vm.Date.Date;

        var employees = await _db.Employees.AsNoTracking().Where(e => e.IsActive).OrderBy(e => e.Name).ToListAsync();
        var empIds = employees.Select(e => e.Id).ToHashSet();

        vm.Rows ??= new();
        vm.Rows = vm.Rows.Where(r => empIds.Contains(r.EmployeeId)).ToList();

        // Replace for date
        var old = await _db.Attendances.Where(a => a.Date == d).ToListAsync();
        _db.Attendances.RemoveRange(old);

        foreach (var row in vm.Rows)
        {
            _db.Attendances.Add(new Attendance
            {
                EmployeeId = row.EmployeeId,
                Date = d,
                Status = row.Status,
                Note = string.IsNullOrWhiteSpace(row.Note) ? null : row.Note.Trim()
            });
        }

        await _db.SaveChangesAsync();

        TempData["ToastSuccess"] = "Attendance saved.";
        return RedirectToAction(nameof(Mark), new { date = d.ToString("yyyy-MM-dd") });
    }

    // ===== ViewModels (inline) =====
    public sealed class AttendanceMarkVm
    {
        public DateTime Date { get; set; } = DateTime.UtcNow.Date;
        public List<AttendanceMarkRowVm> Rows { get; set; } = new();
    }

    public sealed class AttendanceMarkRowVm
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = "";
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        public string? Note { get; set; }
    }
}
 
