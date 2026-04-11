namespace OnlineSalesManagementSystem.Services.Inventory;

public interface IStockService
{
    Task IncreaseStockAsync(int productId, int qty, string refType, int refId, string? note = null, CancellationToken ct = default);
    Task DecreaseStockAsync(int productId, int qty, string refType, int refId, string? note = null, CancellationToken ct = default);
    Task AdjustStockAsync(int productId, int deltaQty, string refType, int refId, string? note = null, CancellationToken ct = default);
}