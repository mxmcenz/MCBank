using MCBank.WebApi.Core.Common;
using Microsoft.AspNetCore.Mvc;

namespace MCBank.WebApi.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();

        return result.ErrorType switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(result.Error),
            ErrorType.Forbidden => new ObjectResult(result.Error) { StatusCode = 403 },
            ErrorType.Validation => new BadRequestObjectResult(result.Error),
            ErrorType.Conflict => new ConflictObjectResult(result.Error),
            ErrorType.Unauthorized => new UnauthorizedObjectResult(result.Error),
            _ => new BadRequestObjectResult(result.Error)
        };
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return ((Result)result).ToActionResult();
    }
}