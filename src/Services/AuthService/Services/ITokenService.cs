using AuthService.Models;

namespace AuthService.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
