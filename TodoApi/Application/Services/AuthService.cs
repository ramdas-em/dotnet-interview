using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TodoApi.Application.DTOs;

namespace TodoApi.Application.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    // Demo user store
    private readonly Dictionary<string, string> _users = new()
    {
        { "admin", "password" } // username: admin, password: password
    };

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? Authenticate(string username, string password)
    {
        if (!_users.TryGetValue(username, out var storedPassword) || storedPassword != password)
            return null;

        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiresInMinutes"]!));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: new[] { new Claim(ClaimTypes.Name, username) },
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
