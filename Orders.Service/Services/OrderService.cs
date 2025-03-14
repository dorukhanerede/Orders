using Orders.BusinessLogic;
using Orders.Service.Clients;
using Orders.Service.Services.Contracts.Responses;
using Orders.Shared.Contracts;
using Orders.Shared.Models.Enums;
using RestSharp;

namespace Orders.Service.Services;

public class OrderService : IOrderService
{
    private readonly ILogger<OrderService> _logger;
    private readonly IChannelEngineClient _channelEngineClient;

    public OrderService(IChannelEngineClient channelEngineClient, ILogger<OrderService> logger)
    {
        _logger = logger;
        _channelEngineClient = channelEngineClient;
    }
    
    public async Task<Result<List<ProcessedOrder>>> GetTopSoldProductsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching IN_PROGRESS orders...");

        var restRequest = new RestRequest("orders");
        restRequest.AddQueryParameter("statuses", OrderStatuses.IN_PROGRESS.ToString());

        var response = await _channelEngineClient.ExecuteAsync<GetOrdersResponse>(restRequest, cancellationToken);
        if (!response.Success)
        {
            _logger.LogError("Failed to fetch orders. Error: {errorCode} - {errors}", response.ErrorCode, response.Errors);
            return Result<List<ProcessedOrder>>.CreateFailure(response.ErrorCode, response.Errors);
        }

        var topProducts = OrderProcessor.GetTopSoldProducts(response.Data!.Content);
        return Result<List<ProcessedOrder>>.CreateSuccess(topProducts);
    }
    
    public async Task<Result> UpdateProductStockAsync(string merchantProductNo, int stock, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating stock for product {merchantProductNo} to {stock}", merchantProductNo, stock);

        var jsonPatch = new[]
        {
            new { value = stock, path = "/Stock", op = "replace" }
        };
        var request = new RestRequest($"products/{merchantProductNo}", Method.Patch);
        request.AddJsonBody(jsonPatch);

        var response = await _channelEngineClient.ExecuteAsync(request, cancellationToken);

        if (!response.Success)
        {
            _logger.LogError("Failed to update stock. Error: {errorCode} - {errors}", response.ErrorCode, response.Errors);
            return Result.CreateFailure(response.ErrorCode, response.Errors);
        }

        return Result.CreateSuccess();
    }
}