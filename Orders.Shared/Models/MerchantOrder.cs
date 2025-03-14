using Orders.Shared.Models.Enums;

namespace Orders.Shared.Models;

public class MerchantOrder
{
    public int Id { get; set; }
    public string ChannelName { get; set; }
    public int ChannelId { get; set; }
    public OrderStatuses Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Email { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime OrderDate { get; set; }
    public List<MerchantOrderLine> Lines { get; set; }
}