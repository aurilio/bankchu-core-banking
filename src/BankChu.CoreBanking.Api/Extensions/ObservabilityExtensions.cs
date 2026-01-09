using BankChu.CoreBanking.Api.Middlewares;
using Serilog;

namespace BankChu.CoreBanking.Api.Extensions;

public static class ObservabilityExtensions
{
    public static IApplicationBuilder UseObservability(this IApplicationBuilder app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        });

        app.UseMiddleware<CorrelationIdMiddleware>();

        return app;
    }
}