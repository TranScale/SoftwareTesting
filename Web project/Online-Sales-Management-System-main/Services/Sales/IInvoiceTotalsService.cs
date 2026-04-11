using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Services.Sales;

public interface IInvoiceTotalsService
{
    void Recalculate(Invoice invoice);
}
