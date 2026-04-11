using System.ComponentModel.DataAnnotations;

namespace OnlineSalesManagementSystem.Domain.Entities;

public class PurchaseItem
{
    public int Id { get; set; }

    [Required]
    public int PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }

    [Required]
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(1, int.MaxValue)]
    public int Qty { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitCost { get; set; }

    [Range(0, double.MaxValue)]
    public decimal LineTotal { get; set; }
}