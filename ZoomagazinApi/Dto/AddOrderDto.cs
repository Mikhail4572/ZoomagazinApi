using System.Text.Json.Serialization;

namespace ZoomagazinApi.Dto;

public class AddOrderDto
{
    [JsonPropertyName("buyerUsername")]
    public string? BuyerUsername { get; set; }

    [JsonPropertyName("sallerUsername")]
    public string? SallerUsername { get; set; }

    [JsonPropertyName("productsInOrders")]
    public List<ProductsInOrderDto> ProductsInOrders { get; set; }
}
