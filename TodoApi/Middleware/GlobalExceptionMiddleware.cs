using System.Diagnostics;
using System.Net;
using System.Text.Json;
using TodoApi.Application.DTOs;

namespace TodoApi.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        _logger.LogError(exception,
            TodoApi.Application.Constants.ErrorMessages.UnhandledExceptionLog,
            traceId, context.Request.Path, context.Request.Method);

        var (statusCode, message) = exception switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest, TodoApi.Application.Constants.ErrorMessages.RequiredArgumentMissing),
            ArgumentException => (HttpStatusCode.BadRequest, TodoApi.Application.Constants.ErrorMessages.InvalidArgument),
            KeyNotFoundException => (HttpStatusCode.NotFound, TodoApi.Application.Constants.ErrorMessages.ResourceNotFound),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, TodoApi.Application.Constants.ErrorMessages.Unauthorized),
            InvalidOperationException => (HttpStatusCode.Conflict, TodoApi.Application.Constants.ErrorMessages.InvalidOperation),
            _ => (HttpStatusCode.InternalServerError, TodoApi.Application.Constants.ErrorMessages.UnexpectedError)
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var errorResponse = new ErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = message,
            Details = context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() == true
                ? exception.Message
                : null,
            TraceId = traceId
        };

        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
