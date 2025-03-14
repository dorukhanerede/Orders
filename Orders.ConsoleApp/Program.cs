using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Orders.ConsoleApp;

class Program
{
    private static readonly HttpClient HttpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5000") }; // Adjust if your API is on a different port

    static async Task Main()
    {
        try
        {
            Console.WriteLine("Fetching top-sold products...");
            var topProducts = await GetTopSoldProductsAsync();
            if (topProducts == null || topProducts.Count == 0)
            {
                Console.WriteLine("No products found.");
                return;
            }

            var productToUpdate = topProducts[0]; // Pick the first product
            Console.WriteLine($"Updating stock for {productToUpdate.ProductName} (GTIN: {productToUpdate.Gtin}) to 25...");

            bool updateSuccess = await UpdateProductStockAsync(productToUpdate.MerchantProductNo, 25);
            if (updateSuccess)
            {
                Console.WriteLine("Stock updated successfully!");
            }
            else
            {
                Console.WriteLine("Failed to update stock.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Fetches the top-sold products from the API
    /// </summary>
    static async Task<List<TopSoldProduct>?> GetTopSoldProductsAsync()
    {
        var response = await HttpClient.GetAsync("/api/orders/top-sold");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to fetch top-sold products. Status: {response.StatusCode}");
            return null;
        }

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<TopSoldProduct>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result?.Data;
    }

    /// <summary>
    /// Updates the stock of a given product using JSON Patch
    /// </summary>
    static async Task<bool> UpdateProductStockAsync(string merchantProductNo, int stock)
    {
        var request = new UpdateProductStockRequest { Stock = stock };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await HttpClient.PatchAsync($"/api/orders/update-stock/{merchantProductNo}", content);

        return response.IsSuccessStatusCode;
    }
}

/// <summary>
/// DTO for API Response
/// </summary>
class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public T Data { get; set; }
}

/// <summary>
/// DTO for Top Sold Products
/// </summary>
class TopSoldProduct
{
    [JsonPropertyName("productName")]
    public string ProductName { get; set; }

    [JsonPropertyName("gtin")]
    public string Gtin { get; set; }

    [JsonPropertyName("totalQuantity")]
    public int TotalQuantity { get; set; }
    
    [JsonPropertyName("merchantProductNo")]
    public string MerchantProductNo { get; set; }
}

/// <summary>
/// DTO for Update Stock Request
/// </summary>
class UpdateProductStockRequest
{
    [JsonPropertyName("stock")]
    public int Stock { get; set; }
}