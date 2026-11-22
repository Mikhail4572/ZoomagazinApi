using ZoomagazinApi.Models;

namespace ZoomagazinApi.Services.AuthService;

public interface IAuthService
{
    string GenerateJwtToken(Account user);
}
