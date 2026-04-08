using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Models;

public class HomeViewModel
{
    public List<Category> TrendingCategories { get; set; } = new();
    public List<Product> TrendingProducts { get; set; } = new();
}