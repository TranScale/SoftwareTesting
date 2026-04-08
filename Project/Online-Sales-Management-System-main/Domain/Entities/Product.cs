using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace OnlineSalesManagementSystem.Domain.Entities;

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    // --- LIÊN KẾT CATEGORY ---
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    // --- LIÊN KẾT UNIT ---
    public int? UnitId { get; set; }
    public Unit? Unit { get; set; }

    // --- LIÊN KẾT BRAND (MỚI) ---
    public int? BrandId { get; set; }
    public Brand? Brand { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CostPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SalePrice { get; set; }

    public string? ImagePath { get; set; }

    public int StockOnHand { get; set; } = 0;
    public int ReorderLevel { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    public bool IsTrending { get; set; } = false;

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    [NotMapped]
    public decimal? DiscountPrice => null;
    public string? Description { get; set; } // Mô tả ngắn
    public string? Content { get; set; }     // Nội dung chi tiết (HTML)
}
