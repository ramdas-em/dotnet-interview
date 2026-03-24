using System.Diagnostics;
using System.Net;
using System.Text.Json;
using TodoApi.DTOs;

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
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
            traceId, context.Request.Path, context.Request.Method);

        var (statusCode, message) = exception switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest, "A required argument was missing."),
            ArgumentException => (HttpStatusCode.BadRequest, "Invalid argument provided."),
            KeyNotFoundException => (HttpStatusCode.NotFound, "The requested resource was not found."),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "You are not authorized to perform this action."),
            InvalidOperationException => (HttpStatusCode.Conflict, "The operation is not valid for the current state."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
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
