using Orders.Shared.Contracts;
using Orders.Shared.Models;

namespace Orders.BusinessLogic;

public class OrderProcessor
{
    public static List<ProcessedOrder> GetTopSoldProducts(List<MerchantOrder> orders)
    {
        return orders
            .SelectMany(order => order.Lines)
            .GroupBy(line => new { line.Description, line.Gtin, line.MerchantProductNo })
            .Select(group => new ProcessedOrder
            {
                ProductName = group.Key.Description,
                Gtin = group.Key.Gtin,
                TotalQuantity = group.Sum(x => x.Quantity),
                MerchantProductNo = group.Key.MerchantProductNo
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(5)
            .ToList();
    }
}