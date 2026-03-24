using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Configuration;
using TodoApi.Application.DTOs;
using TodoApi.Application.Services;
using TodoApi.Controllers;
using Xunit;

namespace TodoApi.Tests;

public class AuthControllerTests
{
    private readonly AuthController _controller;
    private readonly IAuthService _service;

    public AuthControllerTests()
    {
        // Use your real AuthService or a mock as needed
        _service = new AuthService(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
        {
            { "Jwt:Key", "supersecretkey1234567890abcdef12345678" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:ExpiresInMinutes", "60" }
        }).Build());
        _controller = new AuthController(_service, NullLogger<AuthController>.Instance);
    }

    [Fact]
    public void Login_WithValidCredentials_ReturnsToken()
    {
        var request = new LoginRequest { Username = "admin", Password = "password" };
        var result = _controller.Login(request);
        var okResult = Assert.IsType<OkObjectResult>(result);
        dynamic response = okResult.Value;
        Assert.False(string.IsNullOrWhiteSpace((string)response.Token));
    }

    [Fact]
    public void Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var request = new LoginRequest { Username = "admin", Password = "wrong" };
        var result = _controller.Login(request);
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public void Login_WithUnknownUser_ReturnsUnauthorized()
    {
        var request = new LoginRequest { Username = "unknown", Password = "password" };
        var result = _controller.Login(request);
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public void Login_Token_ShouldContainJwtParts()
    {
        var request = new LoginRequest { Username = "admin", Password = "password" };
        var result = _controller.Login(request);
        var okResult = Assert.IsType<OkObjectResult>(result);
        dynamic response = okResult.Value;
        string token = response.Token;
        // JWT should have 3 parts separated by '.'
        Assert.Equal(3, token.Split('.').Length);
    }
}
