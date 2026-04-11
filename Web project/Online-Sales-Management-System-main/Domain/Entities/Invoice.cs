namespace OnlineSalesManagementSystem.Domain.Entities;

public class Invoice
{
    public int Id { get; set; }

    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    // Cho phép bán lẻ (walk-in)
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public decimal SubTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}
