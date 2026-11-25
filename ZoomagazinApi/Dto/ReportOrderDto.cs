using System.Text.Json.Serialization;

namespace ZoomagazinApi.Dto;

public class ReportOrderDto
{
    public int OrderId { get; set; }

    public string? BuyerUsername { get; set; }

    public string? SallerUsername { get; set; }

    public List<ProductsInOrderDto> ProductsInOrder { get; set; }
}
