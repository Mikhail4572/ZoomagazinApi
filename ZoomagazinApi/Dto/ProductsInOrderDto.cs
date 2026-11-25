using System.Text.Json.Serialization;

namespace ZoomagazinApi.Dto
{
    public class ProductsInOrderDto
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
