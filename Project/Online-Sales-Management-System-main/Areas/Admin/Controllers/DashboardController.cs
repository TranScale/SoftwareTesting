using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Services.Security;

namespace OnlineSalesManagementSystem.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = PermissionConstants.PolicyPrefix + PermissionConstants.Modules.Dashboard + "." + PermissionConstants.Actions.Show)]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public DashboardController(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _cache.GetOrCreateAsync("admin:dashboard:stats", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(20);
            entry.SlidingExpiration = TimeSpan.FromSeconds(10);

            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            return new DashboardStats
            {
                AdminsCount = await _db.Users.CountAsync(u => u.IsActive),
                ProductsCount = await _db.Products.CountAsync(p => p.IsActive),
                CustomersCount = await _db.Customers.CountAsync(c => c.IsActive),
                SuppliersCount = await _db.Suppliers.CountAsync(s => s.IsActive),
                EmployeesCount = await _db.Employees.CountAsync(e => e.IsActive),
                PurchasesCount = await _db.Purchases.CountAsync(),
                InvoicesCount = await _db.Invoices.CountAsync(),
                ExpensesCount = await _db.Expenses.CountAsync(x => x.IsActive),
                LowStockCount = await _db.Products.CountAsync(p => p.IsActive && p.StockOnHand <= p.ReorderLevel),
                TodaySales = await _db.Invoices
                    .Where(i => i.InvoiceDate >= today && i.InvoiceDate < today.AddDays(1) && i.Status != Domain.Entities.InvoiceStatus.Cancelled)
                    .SumAsync(i => (decimal?)i.GrandTotal) ?? 0m,
                MonthSales = await _db.Invoices
                    .Where(i => i.InvoiceDate >= monthStart && i.InvoiceDate < monthStart.AddMonths(1) && i.Status != Domain.Entities.InvoiceStatus.Cancelled)
                    .SumAsync(i => (decimal?)i.GrandTotal) ?? 0m
            };
        });

        ViewBag.AdminsCount = stats!.AdminsCount;
        ViewBag.ProductsCount = stats.ProductsCount;
        ViewBag.CustomersCount = stats.CustomersCount;
        ViewBag.SuppliersCount = stats.SuppliersCount;
        ViewBag.EmployeesCount = stats.EmployeesCount;
        ViewBag.PurchasesCount = stats.PurchasesCount;
        ViewBag.InvoicesCount = stats.InvoicesCount;
        ViewBag.ExpensesCount = stats.ExpensesCount;
        ViewBag.LowStockCount = stats.LowStockCount;
        ViewBag.TodaySales = stats.TodaySales;
        ViewBag.MonthSales = stats.MonthSales;

        return View();
    }

    private sealed class DashboardStats
    {
        public int AdminsCount { get; init; }
        public int ProductsCount { get; init; }
        public int CustomersCount { get; init; }
        public int SuppliersCount { get; init; }
        public int EmployeesCount { get; init; }
        public int PurchasesCount { get; init; }
        public int InvoicesCount { get; init; }
        public int ExpensesCount { get; init; }
        public int LowStockCount { get; init; }
        public decimal TodaySales { get; init; }
        public decimal MonthSales { get; init; }
    }
}
 
