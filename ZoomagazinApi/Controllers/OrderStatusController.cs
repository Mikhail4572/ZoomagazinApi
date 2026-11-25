using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomagazinApi.Models;

namespace ZoomagazinApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class OrderStatusController : ControllerBase
{
    private readonly MagazCoolDbContext _context;

    public OrderStatusController(MagazCoolDbContext context) =>
        _context = context;

    [HttpPost("all")]
    public async Task<IResult> GetOrderStatuses()
    {
        var statuses = _context.OrderStatuses.ToList();
        return Results.Ok(statuses);
    }

    [HttpPost("add/{name}")]
    public async Task<IResult> Add(string name)
    {
        _context.OrderStatuses.Add(new()
        {
            Name = name
        });

        _context.SaveChanges();

        return Results.Created();
    }
}
