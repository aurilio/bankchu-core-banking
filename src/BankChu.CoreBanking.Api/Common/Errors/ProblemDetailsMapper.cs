using BankChu.CoreBanking.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace BankChu.CoreBanking.Api.Common.Errors;

public static class ProblemDetailsMapper
{
    public static IResult ToProblemDetails(
        this Error error,
        HttpContext httpContext)
    {
        var statusCode = error.Type switch
        {
            ErrorType.BadRequest => StatusCodes.Status400BadRequest,
            ErrorType.BusinessValidation => StatusCodes.Status422UnprocessableEntity,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Type = $"https://bankchu.com/problems/{error.Code.ToLowerInvariant()}",
            Title = error.Message,
            Status = statusCode,
            Detail = error.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errorCode"] = error.Code;

        return Results.Problem(problemDetails);
    }
}