namespace Orders.Shared.Contracts;

public class ProcessedOrder
{
    public string ProductName { get; set; }
    public string Gtin { get; set; }
    public int TotalQuantity { get; set; }
    public string MerchantProductNo { get; set; }
}