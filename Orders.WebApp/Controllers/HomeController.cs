using Microsoft.AspNetCore.Mvc;
using Orders.WebApp.Models;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Orders.WebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _httpClient;

    public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("http://localhost:5000"); // Update with actual API base URL
    }

    public async Task<IActionResult> Index()
    {
        var topProducts = await GetTopSoldProductsAsync();
        return View(topProducts);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    /// <summary>
    /// Fetches the top-sold products from the API
    /// </summary>
    private async Task<List<TopSoldProduct>> GetTopSoldProductsAsync()
    {
        var response = await _httpClient.GetAsync("/api/orders/top-sold");
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to fetch top-sold products. Status: {response.StatusCode}");
            return new List<TopSoldProduct>();
        }

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<TopSoldProduct>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result?.Data ?? new List<TopSoldProduct>();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStock(string merchantProductNo, int stock)
    {
        var stockUpdateRequest = new { stock };

        var json = JsonSerializer.Serialize(stockUpdateRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PatchAsync($"/api/orders/update-stock/{merchantProductNo}", content);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Stock updated successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to update stock.";
        }

        return RedirectToAction("Index");
    }
}

/// <summary>
/// DTO for API Response
/// </summary>
class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
}

/// <summary>
/// DTO for Top Sold Products
/// </summary>
public class TopSoldProduct
{
    public string ProductName { get; set; }
    public string Gtin { get; set; }
    public int TotalQuantity { get; set; }
    public string MerchantProductNo { get; set; }
}
