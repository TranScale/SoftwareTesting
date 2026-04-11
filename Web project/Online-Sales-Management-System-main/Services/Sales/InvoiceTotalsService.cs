using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Services.Sales;

public sealed class InvoiceTotalsService : IInvoiceTotalsService
{
    public void Recalculate(Invoice invoice)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));

        invoice.Items ??= new List<InvoiceItem>();

        decimal subTotal = 0m;

        foreach (var item in invoice.Items)
        {
            // An toàn dữ liệu
            if (item.Quantity < 0) item.Quantity = 0;
            if (item.UnitPrice < 0) item.UnitPrice = 0;

            item.LineTotal = item.UnitPrice * item.Quantity;
            subTotal += item.LineTotal;
        }

        invoice.SubTotal = subTotal;
        invoice.GrandTotal = subTotal;

        if (invoice.PaidAmount < 0) invoice.PaidAmount = 0;

        invoice.Status = invoice.PaidAmount >= invoice.GrandTotal
            ? InvoiceStatus.Paid
            : InvoiceStatus.Unpaid;
    }
}
