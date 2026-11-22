using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Security.Claims;
using ZoomagazinApi.Dto;
using ZoomagazinApi.Models;

namespace ZoomagazinApi.Controllers;


[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly MagazCoolDbContext _context;

    public OrderController(MagazCoolDbContext context) =>
        _context = context;

    [HttpGet("get")]
    public async Task<IResult> GetOrders()
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(username))
            return Results.StatusCode(404);

        var orders = _context.Orders.Where(x => x.BuyerUsername == username).ToList();

        return Results.Ok(orders);
    }

    [HttpGet("getAll")]
    [Authorize(Roles = "admin")]
    public async Task<IResult> GetAllOrders()
    {
        var orders = _context.Orders.ToList();

        return Results.Ok(orders);
    }

    [HttpPost("add")]
    public async Task<IResult> CreateOrder([FromBody] AddOrderDto addOrderDto)
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(username))
            return Results.StatusCode(404);

        if (addOrderDto.ProductsInOrders.Count == 0)
        {
            return Results.Problem(
                title: "пустой заказ",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var ids = addOrderDto.ProductsInOrders.Select(x => x.ProductId);

        bool flag = _context.Products.Count(x => ids.Contains(x.Id)) == ids.Count();

        if (!flag)
        {
            return Results.Problem(
                title: "Один или более продуктов из заказа не найдены",
                statusCode: StatusCodes.Status400BadRequest);
        }

        Order order = new()
        {
            CreatedAt = DateTime.UtcNow,
            BuyerUsername = addOrderDto.BuyerUsername,
            OrderStatusId = 1,
            SallerLogin = username
        };


        try
        {
            _context.Orders.Add(order);
            _context.SaveChanges();

            var orderProducts = addOrderDto.ProductsInOrders.Select(x => new OrderProduct
            {
                ProductId = x.ProductId,
                OrderId = order.Id,
                Count = x.Count
            }).ToList();

            _context.OrderProducts.AddRange(orderProducts);

            _context.SaveChanges();

            return Results.Created();
        }
        catch (Exception ex)
        {
            return Results.Problem(
                type: ex.ToString(),
                title: ex.Message,
                detail: ex.InnerException?.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
    }

    [HttpPost("cancel/{id:int}")]
    public async Task<IResult> CancelOrder(int id)
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(username))
            return Results.StatusCode(404);

        var order = _context.Orders.FirstOrDefault(x => x.Id == id && x.BuyerUsername == username);

        if (order is null)
        {
            return Results.Problem(
                title: "такой заказ не найден",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        order.OrderStatusId = 4;
        _context.SaveChanges();

        return Results.Ok();
    }

    [Authorize(Roles = "admin")]
    [HttpPost("cancelOrder/{id:int}")]
    public async Task<IResult> CancelOrderAdmin(int id)
    {
        var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(username))
            return Results.StatusCode(404);

        var order = _context.Orders.FirstOrDefault(x => x.Id == id);

        if (order is null)
        {
            return Results.Problem(
                title: "такой заказ не найден",
                statusCode: StatusCodes.Status404NotFound
            );
        }

        order.OrderStatusId = 4;
        _context.SaveChanges();

        return Results.Ok();
    }

    [Authorize(Roles = "admin")]
    [HttpPost("updateDiscount")]
    public async Task<IResult> UpdateDiscount([FromBody] UpdateDiscountDto updateDiscount)
    {
        var product = _context.Products.FirstOrDefault(x => x.Id == updateDiscount.Productid);

        if (product is null)
            return Results.StatusCode(404);

        product.Discount = updateDiscount.Discount;

        _context.SaveChanges();

        return Results.Empty;
    }
}
