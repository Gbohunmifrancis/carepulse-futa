using System.Collections.Generic;

namespace FutaMedical.Application.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public int StatusCode { get; init; }
    public List<string>? Errors { get; init; }

    public static ApiResponse<T> Ok(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 200
        };
    }

    public static ApiResponse<T> Created(T data, string message = "Created successfully")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = 201
        };
    }

    public static ApiResponse<T> NotFound(string message = "Resource not found", List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 404,
            Errors = errors
        };
    }

    public static ApiResponse<T> BadRequest(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 400,
            Errors = errors
        };
    }

    public static ApiResponse<T> Conflict(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 409
        };
    }

    public static ApiResponse<T> ServerError(string message = "An error occurred", List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            StatusCode = 500,
            Errors = errors
        };
    }

    // Keep compatibility for static wrappers if needed
    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful") => Ok(data, message);
    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null) => BadRequest(message, errors);
}
