using System.Security.Claims;
using System.Security.Cryptography;
using ChatApp.Data;
using ChatApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChatApp.Services;

public class AuthService(AppDbContext db, IOptions<AdminSettings> adminOptions)
{
    public bool ValidateAdmin(string username, string password)
    {
        var admin = adminOptions.Value;
        return username == admin.Username && password == admin.Password;
    }

    public async Task<(User user, string token)> IssueUserTokenAsync(string nickname)
    {
        var existing = await db.Users.FirstOrDefaultAsync(u => u.Nickname == nickname);
        if (existing is not null)
        {
            return (existing, existing.AuthToken);
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(40));
        var user = new User
        {
            Nickname = nickname.Trim(),
            AuthToken = token,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (user, token);
    }

    public async Task<User?> GetUserByTokenAsync(string token)
    {
        return await db.Users.FirstOrDefaultAsync(u => u.AuthToken == token);
    }

    public ClaimsPrincipal CreatePrincipalForUser(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Nickname),
            new("nickname", user.Nickname),
            new("role", "user")
        };

        var identity = new ClaimsIdentity(claims, "cookie");
        return new ClaimsPrincipal(identity);
    }

    public ClaimsPrincipal CreatePrincipalForAdmin(string username)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "admin"),
            new(ClaimTypes.Name, username),
            new("role", "admin")
        };

        var identity = new ClaimsIdentity(claims, "cookie");
        return new ClaimsPrincipal(identity);
    }
}
