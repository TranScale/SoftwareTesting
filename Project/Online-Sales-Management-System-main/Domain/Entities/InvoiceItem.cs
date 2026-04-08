using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineSalesManagementSystem.Domain.Entities;

public class InvoiceItem
{
    public int Id { get; set; }

    [Required]
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    [Required]
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;
    public decimal LineTotal { get; set; }

}
