using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.Service.Clients;
using Orders.Service.Services;
using Orders.Service.Services.Contracts.Responses;
using Orders.Shared.Contracts;
using Orders.Shared.Models;

namespace Orders.Service.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IChannelEngineClient> _mockClient;
    private readonly OrderService _orderService;
    private readonly Mock<ILogger<OrderService>> _mockLogger;

    public OrderServiceTests()
    {
        _mockClient = new Mock<IChannelEngineClient>();
        _mockLogger = new Mock<ILogger<OrderService>>();
        _orderService = new OrderService(_mockClient.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task GetTopSoldProductsAsync_ReturnsSortedOrders_WhenSuccessful()
    {
        var fakeResponse = new GetOrdersResponse
        {
            TotalCount = 5,
            Content = new List<MerchantOrder>
            {
                new() { Id = 123, ChannelName = "Amazon", Lines =
                    [new() { Description = "Keyboard", Gtin = "123", Quantity = 8, MerchantProductNo = "K123" }]
                },
                new() { Id = 124, ChannelName = "eBay", Lines =
                    [new() { Description = "Mouse", Gtin = "124", Quantity = 5, MerchantProductNo = "M124" }]
                },
                new() { Id = 125, ChannelName = "BestBuy", Lines =
                    [new() { Description = "GPU", Gtin = "125", Quantity = 2, MerchantProductNo = "G125" }]
                },
                new() { Id = 126, ChannelName = "Newegg", Lines =
                    [new() { Description = "CPU", Gtin = "126", Quantity = 1, MerchantProductNo = "C126" }]
                },
                new() { Id = 127, ChannelName = "eBay", Lines =
                    [new() { Description = "Mouse", Gtin = "124", Quantity = 2, MerchantProductNo = "M124" }] 
                }
            }
        };

        _mockClient
            .Setup(client => client.ExecuteAsync<GetOrdersResponse>(It.IsAny<RestSharp.RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<GetOrdersResponse>.CreateSuccess(fakeResponse));

        // Act
        var result = await _orderService.GetTopSoldProductsAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(4, result.Data.Count);

        Assert.Equal("Keyboard", result.Data[0].ProductName);
        Assert.Equal(8, result.Data[0].TotalQuantity);

        Assert.Equal("Mouse", result.Data[1].ProductName);
        Assert.Equal(7, result.Data[1].TotalQuantity);

        Assert.Equal("GPU", result.Data[2].ProductName);
        Assert.Equal(2, result.Data[2].TotalQuantity);

        Assert.Equal("CPU", result.Data[3].ProductName);
        Assert.Equal(1, result.Data[3].TotalQuantity);
    }
    
    [Fact]
    public async Task GetTopSoldProductsAsync_ReturnsEmptyList_WhenNoOrdersExist()
    {
        // Arrange
        var fakeResponse = new GetOrdersResponse
        {
            TotalCount = 0,
            Content = new List<MerchantOrder>()
        };

        _mockClient
            .Setup(client => client.ExecuteAsync<GetOrdersResponse>(It.IsAny<RestSharp.RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<GetOrdersResponse>.CreateSuccess(fakeResponse));

        // Act
        var result = await _orderService.GetTopSoldProductsAsync(CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }
    
    [Fact]
    public async Task GetTopSoldProductsAsync_ReturnsFailure_WhenApiFails()
    {
        // Arrange
        _mockClient
            .Setup(client => client.ExecuteAsync<GetOrdersResponse>(It.IsAny<RestSharp.RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<GetOrdersResponse>.CreateFailure(HttpStatusCode.InternalServerError, new List<ErrorObject>()));

        // Act
        var result = await _orderService.GetTopSoldProductsAsync(CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Data);
    }
    
    [Fact]
    public async Task UpdateProductStockAsync_ReturnsSuccess_WhenApiRespondsSuccessfully()
    {
        // Arrange
        string merchantProductNo = "K123";
        int newStock = 25;

        _mockClient
            .Setup(client => client.ExecuteAsync(It.IsAny<RestSharp.RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.CreateSuccess());

        // Act
        var result = await _orderService.UpdateProductStockAsync(merchantProductNo, newStock, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task UpdateProductStockAsync_ReturnsFailure_WhenApiFails()
    {
        // Arrange
        string merchantProductNo = "K123";
        int newStock = 25;

        _mockClient
            .Setup(client => client.ExecuteAsync(It.IsAny<RestSharp.RestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.CreateFailure(HttpStatusCode.InternalServerError, new List<ErrorObject>()));

        // Act
        var result = await _orderService.UpdateProductStockAsync(merchantProductNo, newStock, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
    }
}