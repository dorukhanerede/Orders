using Orders.Shared.Contracts;

namespace Orders.Service.Services;

public interface IOrderService
{
    Task<Result<List<ProcessedOrder>>> GetTopSoldProductsAsync(CancellationToken cancellationToken);
    Task<Result> UpdateProductStockAsync(string merchantProductNo, int stock, CancellationToken cancellationToken);
}