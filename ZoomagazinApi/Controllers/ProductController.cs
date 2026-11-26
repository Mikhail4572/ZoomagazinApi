using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZoomagazinApi.Dto;
using ZoomagazinApi.Models;

namespace ZoomagazinApi.Controllers;


[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly MagazCoolDbContext _context;

    public ProductController(MagazCoolDbContext context) =>
        _context = context;

    [HttpGet("getAll")]
    public async Task<IResult> GetProducts()
    {
        var products = _context.Products.Select(x => new
        {
            x.Id,
            x.Name,
            Price = x.Price - (x.Price / 100 * ((decimal)x.Discount)),
            x.Count
        }).ToList();

        return Results.Ok(products);
    }

    [Authorize(Roles = "admin")]
    [HttpGet("weeklyStatistics/{productid:int}")]
    public async Task<IResult> WeeklyStatistics(int productid)
    {

        var date = DateTime.UtcNow.Date.AddDays(-7);

        var statistics = _context.OrderProducts
            .Where(x => x.ProductId == productid && x.Order.CreatedAt >= date).GroupBy(x => x.Order.CreatedAt.Date)
            .Select(x => new
            {
                Date = DateOnly.FromDateTime(x.Key),
                Count = x.Sum(y => y.Count)
            })
            .ToDictionary(x => x.Date, x => x.Count);

        return Results.Ok(statistics);
    }

    [Authorize(Roles = "admin")]
    [HttpPost("create")]
    public async Task<IResult> CreateProduct([FromBody] ProductDto add)
    {
        if (!ModelState.IsValid)
        {
            return Results.Problem(
                title: "данные не валидны",
                extensions: ModelState.Where(x => x.Value != null).ToDictionary(
                   x => x.Key,
                   x => (object?)x.Value),
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        _context.Products.Add(new()
        {
            Discount = add.Discount,
            Count = add.Count,
            Name = add.Name,
            Price = add.Price
        });

        _context.SaveChanges();

        return Results.Created();
    }
}
