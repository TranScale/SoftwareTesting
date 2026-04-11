using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;

namespace OnlineSalesManagementSystem.Controllers.Api;

[ApiController]
[Route("api/v1/catalog")]
public sealed class CatalogController : ControllerBase
{
    private static readonly HashSet<string> AllowedSorts = new(StringComparer.OrdinalIgnoreCase)
    {
        "default",
        "price_asc",
        "price_desc",
        "newest"
    };

    private readonly ApplicationDbContext _db;

    public CatalogController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? search,
        [FromQuery] int? categoryId,
        [FromQuery] int? brandId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 9,
        CancellationToken ct = default)
    {
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        sort = string.IsNullOrWhiteSpace(sort) ? "default" : sort.Trim().ToLowerInvariant();

        if (page < 1) return ValidationError("page", "Page must be greater than 0.");
        if (pageSize < 1 || pageSize > 100) return ValidationError("pageSize", "Page size must be between 1 and 100.");
        if (minPrice.HasValue && minPrice.Value < 0) return ValidationError("minPrice", "Minimum price cannot be negative.");
        if (maxPrice.HasValue && maxPrice.Value < 0) return ValidationError("maxPrice", "Maximum price cannot be negative.");
        if (minPrice.HasValue && maxPrice.HasValue && minPrice.Value > maxPrice.Value)
            return ValidationError("priceRange", "Minimum price cannot be greater than maximum price.");
        if (!AllowedSorts.Contains(sort))
            return ValidationError("sort", "Sort must be one of: default, price_asc, price_desc, newest.");

        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || p.SKU.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);

        if (minPrice.HasValue)
            query = query.Where(p => p.SalePrice >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.SalePrice <= maxPrice.Value);

        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.SalePrice),
            "price_desc" => query.OrderByDescending(p => p.SalePrice),
            "newest" => query.OrderByDescending(p => p.Id),
            _ => query.OrderBy(p => p.Name)
        };

        var totalItems = await query.CountAsync(ct);
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        if (totalPages > 0 && page > totalPages)
            return ValidationError("page", $"Page exceeds total pages ({totalPages}).");

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItem(
                p.Id,
                p.SKU,
                p.Name,
                p.CategoryId,
                p.Category != null ? p.Category.Name : null,
                p.BrandId,
                p.Brand != null ? p.Brand.Name : null,
                p.SalePrice,
                p.StockOnHand,
                p.ReorderLevel,
                p.IsTrending,
                p.ImagePath))
            .ToListAsync(ct);

        return Ok(new ProductListResponse(page, pageSize, totalItems, totalPages, items));
    }

    [HttpGet("products/{id:int}")]
    public async Task<IActionResult> GetProduct(int id, CancellationToken ct = default)
    {
        if (id <= 0) return ValidationError("id", "Id must be greater than 0.");

        var product = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Unit)
            .Where(p => p.IsActive && p.Id == id)
            .Select(p => new ProductDetailsResponse(
                p.Id,
                p.SKU,
                p.Name,
                p.Description,
                p.Content,
                p.CategoryId,
                p.Category != null ? p.Category.Name : null,
                p.BrandId,
                p.Brand != null ? p.Brand.Name : null,
                p.UnitId,
                p.Unit != null ? p.Unit.Name : null,
                p.CostPrice,
                p.SalePrice,
                p.StockOnHand,
                p.ReorderLevel,
                p.IsTrending,
                p.ImagePath))
            .FirstOrDefaultAsync(ct);

        if (product == null)
            return NotFound(new ApiErrorResponse("not_found", "Product not found.", "id"));

        return Ok(product);
    }

    [HttpGet("trending")]
    public async Task<IActionResult> GetTrending(CancellationToken ct = default)
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive && c.IsTrending)
            .OrderByDescending(c => c.Id)
            .Take(8)
            .Select(c => new LookupItem(c.Id, c.Name, null))
            .ToListAsync(ct);

        var products = await _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Where(p => p.IsActive && p.IsTrending)
            .OrderByDescending(p => p.Id)
            .Take(8)
            .Select(p => new ProductListItem(
                p.Id,
                p.SKU,
                p.Name,
                p.CategoryId,
                p.Category != null ? p.Category.Name : null,
                p.BrandId,
                p.Brand != null ? p.Brand.Name : null,
                p.SalePrice,
                p.StockOnHand,
                p.ReorderLevel,
                p.IsTrending,
                p.ImagePath))
            .ToListAsync(ct);

        return Ok(new TrendingResponse(categories, products));
    }

    [HttpGet("filters")]
    public async Task<IActionResult> GetFilters(CancellationToken ct = default)
    {
        var categories = await _db.Categories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new LookupItem(
                c.Id,
                c.Name,
                _db.Products.Count(p => p.IsActive && p.CategoryId == c.Id)))
            .ToListAsync(ct);

        var brands = await _db.Brands
            .AsNoTracking()
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .Select(b => new LookupItem(
                b.Id,
                b.Name,
                _db.Products.Count(p => p.IsActive && p.BrandId == b.Id)))
            .ToListAsync(ct);

        return Ok(new FiltersResponse(categories, brands));
    }

    private BadRequestObjectResult ValidationError(string target, string message)
        => BadRequest(new ApiErrorResponse("validation_error", message, target));

    private sealed record ApiErrorResponse(string Code, string Message, string Target);
    private sealed record LookupItem(int Id, string Name, int? ProductCount);
    private sealed record ProductListItem(
        int Id,
        string SKU,
        string Name,
        int? CategoryId,
        string? CategoryName,
        int? BrandId,
        string? BrandName,
        decimal SalePrice,
        int StockOnHand,
        int ReorderLevel,
        bool IsTrending,
        string? ImagePath);
    private sealed record ProductListResponse(
        int Page,
        int PageSize,
        int TotalItems,
        int TotalPages,
        IReadOnlyList<ProductListItem> Items);
    private sealed record ProductDetailsResponse(
        int Id,
        string SKU,
        string Name,
        string? Description,
        string? Content,
        int? CategoryId,
        string? CategoryName,
        int? BrandId,
        string? BrandName,
        int? UnitId,
        string? UnitName,
        decimal CostPrice,
        decimal SalePrice,
        int StockOnHand,
        int ReorderLevel,
        bool IsTrending,
        string? ImagePath);
    private sealed record TrendingResponse(
        IReadOnlyList<LookupItem> Categories,
        IReadOnlyList<ProductListItem> Products);
    private sealed record FiltersResponse(
        IReadOnlyList<LookupItem> Categories,
        IReadOnlyList<LookupItem> Brands);
}
