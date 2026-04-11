using Microsoft.EntityFrameworkCore;
using OnlineSalesManagementSystem.Data;
using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Services.Inventory;

public sealed class StockService : IStockService
{
    private readonly ApplicationDbContext _db;

    public StockService(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task IncreaseStockAsync(int productId, int qty, string refType, int refId, string? note = null, CancellationToken ct = default)
        => ApplyDeltaAsync(productId, Math.Abs(qty), StockMovementType.In, refType, refId, note, ct);

    public Task DecreaseStockAsync(int productId, int qty, string refType, int refId, string? note = null, CancellationToken ct = default)
        => ApplyDeltaAsync(productId, -Math.Abs(qty), StockMovementType.Out, refType, refId, note, ct);

    public Task AdjustStockAsync(int productId, int deltaQty, string refType, int refId, string? note = null, CancellationToken ct = default)
        => ApplyDeltaAsync(productId, deltaQty, StockMovementType.Adjust, refType, refId, note, ct);

    private async Task ApplyDeltaAsync(
    int productId,
    int deltaQty,
    StockMovementType movementType,
    string refType,
    int refId,
    string? note,
    CancellationToken ct)
    {
        if (productId <= 0) throw new ArgumentOutOfRangeException(nameof(productId));
        if (deltaQty == 0) return;

        refType = string.IsNullOrWhiteSpace(refType) ? "System" : refType.Trim();

        const int maxAttempts = 3;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            await using var tx = await _db.Database.BeginTransactionAsync(ct);

            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
                if (product is null) throw new InvalidOperationException("Product not found.");

                // Business rule: prevent oversell
                var newStock = product.StockOnHand + deltaQty;
                if (newStock < 0)
                {
                    throw new InvalidOperationException("Insufficient stock. Please refresh and try again.");
                }

                product.StockOnHand = newStock;

                _db.StockMovements.Add(new StockMovement
                {
                    ProductId = productId,
                    MovementDate = DateTime.UtcNow, // store UTC
                    Type = movementType,
                    Qty = Math.Abs(deltaQty),
                    RefType = refType,
                    RefId = refId,
                    Note = note
                });

                await _db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return;
            }
            catch (DbUpdateConcurrencyException)
            {
                await tx.RollbackAsync(ct);

                if (attempt == maxAttempts)
                {
                    throw new InvalidOperationException("Stock was updated by another user. Please retry.");
                }

                // Clear tracked entities so next attempt reloads fresh values
                _db.ChangeTracker.Clear();
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
    }
}

