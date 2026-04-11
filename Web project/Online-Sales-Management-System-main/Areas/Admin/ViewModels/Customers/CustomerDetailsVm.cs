using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Areas.Admin.ViewModels.Customers;

public class CustomerDetailsVm
{
    public Customer Customer { get; set; } = new();

    public List<CustomerOrderRowVm> Orders { get; set; } = new();

    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalOutstanding { get; set; }

    public CustomerOrderRowVm? LastOrder { get; set; }
}

public class CustomerOrderRowVm
{
    public int Id { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public InvoiceStatus Status { get; set; }

    public int ItemCount { get; set; }
    public string ProductPreview { get; set; } = string.Empty;

    public decimal SubTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance { get; set; }
}
