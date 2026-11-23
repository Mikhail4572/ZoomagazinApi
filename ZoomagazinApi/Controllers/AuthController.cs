using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZoomagazinApi.Dto;
using ZoomagazinApi.Models;
using ZoomagazinApi.Services.AuthService;

namespace ZoomagazinApi.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly MagazCoolDbContext _context;
    private readonly IAuthService _authService;

    public AuthController(MagazCoolDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    //http://localhost:8080/api/Auth/check
    [HttpGet("check")]
    public async Task<IResult> Check()
    {
        Console.WriteLine($"\n\n{nameof(Check)} is caller\n\n");
        return Results.Ok();
    }

    [HttpPost("reg")]
    public async Task<IResult> Reg([FromBody] RegDto loginDto)
    {
        try
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

            if (await _context.Accounts.AnyAsync(x => x.Username == loginDto.Username))
            {
                Results.Problem(
                    title: "Такой логин уже занят",
                    statusCode: StatusCodes.Status409Conflict
                );
            }

            if (await _context.Accounts.AnyAsync(x => x.Email == loginDto.Email))
            {
                Results.Problem(
                    title: "Такая почта уже занята",
                    statusCode: StatusCodes.Status409Conflict
                );
            }

            Account account = new()
            {
                Username = loginDto.Username,
                Email = loginDto.Email,
                Password = loginDto.Password,
                Role = (await _context.Accounts.AnyAsync()) ? "user" : "admin"
            };


            _context.Accounts.Add(account);
            _context.SaveChanges();

            string token = _authService.GenerateJwtToken(account);

            return Results.Ok(token);
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: ex.Message,
                detail: ex.InnerException?.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }

    [HttpPost("login")]
    public async Task<IResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await _context.Accounts.FirstOrDefaultAsync(x => x.Username == loginDto.Username && x.Password == loginDto.Password);

        if (user is null)
        {
            return Results.Problem(
                title: "Неверный логин или пароль",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        string token = _authService.GenerateJwtToken(user);

        return Results.Ok(token);
    }

    [Authorize(Roles = "admin")]
    [HttpGet("getUserByUsername/{username}")]
    public async Task<IResult> GetUserByUsername(string username)
    {
        var user = _context.Accounts.FirstOrDefault(x => x.Username == username);

        return user is not null ? Results.Ok(new AccountAdminDto
        {
            Username = user.Username,
            Email = user.Email,
            Password = user.Password,
            Role = user.Role
        }) :
        Results.StatusCode(StatusCodes.Status404NotFound);
    }

    [Authorize(Roles = "admin")]
    [HttpGet("getUserByEmail/{email}")]
    public async Task<IResult> GetUserByEmail(string email)
    {
        var user = _context.Accounts.FirstOrDefault(x => x.Email == email);

        return user is not null ? Results.Ok(new AccountAdminDto
        {
            Username = user.Username,
            Email = user.Email,
            Password = user.Password,
            Role = user.Role
        }) :
        Results.StatusCode(StatusCodes.Status404NotFound);
    }
}
