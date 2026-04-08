using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Models;

namespace OnlineSalesManagementSystem.Controllers;

public class ProductController : Controller
{
    private readonly ApplicationDbContext _db;

    public ProductController(ApplicationDbContext db)
    {
        _db = db;
    }

    // GET: /Product
    public async Task<IActionResult> Index(
        string? search,
        int? category,
        int? brand,
        decimal? min,
        decimal? max,
        string? sort,
        int page = 1)
    {
        int pageSize = 9; // Số sản phẩm trên 1 trang

        // 1. Truy vấn cơ bản
        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsActive);

        // 2. Áp dụng bộ lọc
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(p => p.Name.Contains(search) || p.SKU.Contains(search));
        }

        if (category.HasValue)
            query = query.Where(p => p.CategoryId == category.Value);

        if (brand.HasValue)
            query = query.Where(p => p.BrandId == brand.Value);

        // Lọc giá: Nhập 100 -> Hiểu là 100,000
        if (min.HasValue) query = query.Where(p => p.SalePrice >= (min.Value * 1000));
        if (max.HasValue) query = query.Where(p => p.SalePrice <= (max.Value * 1000));

        // 3. Sắp xếp
        sort = string.IsNullOrEmpty(sort) ? "default" : sort;
        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.SalePrice),
            "price_desc" => query.OrderByDescending(p => p.SalePrice),
            "newest" => query.OrderByDescending(p => p.Id),
            _ => query.OrderBy(p => p.Name)
        };

        // 4. Phân trang
        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 5. Lấy dữ liệu Sidebar
        var categoriesWithCount = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Count = _db.Products.Count(p => p.CategoryId == c.Id && p.IsActive)
            })
            .ToListAsync();

        var brands = await _db.Brands.AsNoTracking().Where(b => b.IsActive).ToListAsync();

        var model = new ShopViewModel
        {
            Products = products,
            CategoriesWithCount = categoriesWithCount,
            Brands = brands,
            SearchTerm = search,
            CategoryId = category,
            BrandId = brand,
            MinPrice = min,
            MaxPrice = max,
            SortBy = sort,
            CurrentPage = page,
            TotalPages = totalPages
        };

        return View(model);
    }

    // GET: /Product/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) return NotFound();

        return View(product);
    }
}