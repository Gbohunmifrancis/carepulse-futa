using FutaMedical.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace FutaMedical.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ReturnResult<T>(ApiResponse<T> result)
        => StatusCode(result.StatusCode, result);
}
