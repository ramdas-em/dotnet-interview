using TodoApi.Application.DTOs;

namespace TodoApi.Application.Services;

public interface IAuthService
{
    string? Authenticate(string username, string password);
}
