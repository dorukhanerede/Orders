using Microsoft.AspNetCore.Mvc;
using Orders.Service.Controllers.OrdersController.Contracts.Requests;
using Orders.Service.Helpers;
using Orders.Service.Services;

namespace Orders.Service.Controllers.OrdersController;

[Route("api/orders")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }
    
    [HttpGet("top-sold")]
    public async Task<IActionResult> GetTopSoldProducts(CancellationToken cancellationToken)
    {
        var result = await _orderService.GetTopSoldProductsAsync(cancellationToken);
        if (!result.Success)
        {
            _logger.LogError("OrderService failed. Error Code: {errorCode} - Errors: {errors}", result.ErrorCode,
                result.Errors);
            return ApiActionResult.Build(result);
        }
        return ApiActionResult.Build(result);
    }
    
    [HttpPatch("update-stock/{merchantProductNo}")]
    public async Task<IActionResult> UpdateProductStock([FromRoute] string merchantProductNo, [FromBody] UpdateProductStockRequest request, CancellationToken cancellationToken)
    {
        var result = await _orderService.UpdateProductStockAsync(merchantProductNo, request.Stock, cancellationToken);
        if (!result.Success)
        {
            _logger.LogError("OrderService failed. Error Code: {errorCode} - Errors: {errors}", result.ErrorCode,
                result.Errors);
            return ApiActionResult.Build(result);
        }
        return ApiActionResult.Build(result);
    }
}