using Microsoft.AspNetCore.Mvc;

namespace BankChu.CoreBanking.Api.Extensions;

public static class ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(
        this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var problem = new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred.",
                    Status = StatusCodes.Status500InternalServerError,
                    Instance = context.Request.Path
                };

                await context.Response.WriteAsJsonAsync(problem);
            });
        });

        return app;
    }
}