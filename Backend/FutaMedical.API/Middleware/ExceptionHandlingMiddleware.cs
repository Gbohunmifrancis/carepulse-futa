using System.Net;
using System.Text.Json;
using FluentValidation;
using FutaMedical.API.Common;

namespace FutaMedical.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            ValidationException validationException => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Response = ApiResponse<object>.ErrorResponse(
                    "Validation failed",
                    validationException.Errors.Select(e => e.ErrorMessage).ToList())
            },
            UnauthorizedAccessException => new
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Response = ApiResponse<object>.ErrorResponse(
                    exception.Message,
                    new List<string> { "Unauthorized access" })
            },
            KeyNotFoundException => new
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Response = ApiResponse<object>.ErrorResponse(
                    "Resource not found",
                    new List<string> { exception.Message })
            },
            _ => new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Response = ApiResponse<object>.ErrorResponse(
                    "An internal error occurred",
                    new List<string> { exception.Message })
            }
        };

        response.StatusCode = errorResponse.StatusCode;
        await response.WriteAsync(JsonSerializer.Serialize(errorResponse.Response));
    }
}
