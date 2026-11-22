using System.Text.Json.Serialization;

namespace ZoomagazinApi.Dto;

public class AddOrderDto
{
    [JsonPropertyName("buyerUsername")]
    public string? BuyerUsername { get; set; }

    [JsonPropertyName("productsInOrders")]
    public List<ProductsInOrder> ProductsInOrders { get; set; }
}


public class ProductsInOrder
{
    [JsonPropertyName("productId")]
    public int ProductId { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
