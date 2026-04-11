using System.ComponentModel.DataAnnotations;

namespace OnlineSalesManagementSystem.Domain.Entities;

public enum PurchaseStatus
{
    Draft = 0,
    Received = 1,
    Cancelled = 2
}

public class Purchase
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string PurchaseNo { get; set; } = string.Empty;

    [Required]
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

    [Range(0, double.MaxValue)]
    public decimal SubTotal { get; set; }

    [Range(0, double.MaxValue)]
    public decimal GrandTotal { get; set; }

    public PurchaseStatus Status { get; set; } = PurchaseStatus.Draft;

    public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
}