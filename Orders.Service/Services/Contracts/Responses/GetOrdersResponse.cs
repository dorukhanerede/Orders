using Orders.Shared.Models;

namespace Orders.Service.Services.Contracts.Responses;

public class GetOrdersResponse
{
    public int Count { get; set; }
    public int TotalCount { get; set; }
    public int StatusCode { get; set; }
    public List<MerchantOrder> Content { get; set; } = new();
}