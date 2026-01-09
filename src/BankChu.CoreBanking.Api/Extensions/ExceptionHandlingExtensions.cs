using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace BankChu.CoreBanking.Api.Extensions;

public static class ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                var userId = context.User?.Identity?.Name;

                Log.Error(
                    exception,
                    "Unhandled exception | Path={Path} | Method={Method} | User={UserId}",
                    context.Request.Path,
                    context.Request.Method,
                    userId);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var problem = new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred.",
                    Status = StatusCodes.Status500InternalServerError,
                    Instance = context.Request.Path
                };

                if (context.Response.Headers.TryGetValue("X-Correlation-Id", out var correlationId))
                {
                    problem.Extensions["correlationId"] = correlationId.ToString();
                }

                await context.Response.WriteAsJsonAsync(problem);
            });
        });

        return app;
    }
}