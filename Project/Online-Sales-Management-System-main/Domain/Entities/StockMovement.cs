using System.ComponentModel.DataAnnotations;

namespace OnlineSalesManagementSystem.Domain.Entities;

public enum StockMovementType
{
    In = 0,
    Out = 1,
    Adjust = 2
}

public class StockMovement
{
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public DateTime MovementDate { get; set; } = DateTime.UtcNow;

    public StockMovementType Type { get; set; } = StockMovementType.Adjust;

    public int Qty { get; set; }

    [MaxLength(50)]
    public string RefType { get; set; } = string.Empty;

    public int RefId { get; set; }

    public string? Note { get; set; }
}