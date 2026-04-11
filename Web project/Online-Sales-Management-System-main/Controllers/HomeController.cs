using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Models;
using System.Diagnostics;

namespace OnlineSalesManagementSystem.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IMemoryCache _cache;

    public HomeController(ApplicationDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = await _cache.GetOrCreateAsync("home:index:trending", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            entry.SlidingExpiration = TimeSpan.FromMinutes(2);

            var trendingCats = await _db.Categories
                .AsNoTracking()
                .Where(c => c.IsActive && c.IsTrending)
                .OrderByDescending(c => c.Id)
                .Take(8)
                .ToListAsync();

            var trendingProds = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.IsTrending)
                .OrderByDescending(p => p.Id)
                .Take(8)
                .ToListAsync();

            return new HomeViewModel
            {
                TrendingCategories = trendingCats,
                TrendingProducts = trendingProds
            };
        });

        return View(viewModel!);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
