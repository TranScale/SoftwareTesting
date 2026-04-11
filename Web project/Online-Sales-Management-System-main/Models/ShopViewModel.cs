using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Models;

// Class phụ để chứa thông tin danh mục + số lượng sản phẩm
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ShopViewModel
{
    public List<Product> Products { get; set; } = new();
    public List<CategoryDto> CategoriesWithCount { get; set; } = new();
    public List<Brand> Brands { get; set; } = new();

    // Các biến lưu trạng thái bộ lọc
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public int? BrandId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string SortBy { get; set; } = "default";

    // Phân trang
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}