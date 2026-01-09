using Serilog;
using Serilog.Exceptions;

namespace BankChu.CoreBanking.Api.Extensions;

public static class LoggingExtensions
{
    public static IHostBuilder AddSerilogLogging(this IHostBuilder host, IConfiguration configuration)
    {
        var seqUrl = configuration["Observability:Seq:Url"];

        var loggerConfig = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .WriteTo.Console();

        if (!string.IsNullOrWhiteSpace(seqUrl))
        {
            loggerConfig.WriteTo.Seq(seqUrl);
        }

        Log.Logger = loggerConfig.CreateLogger();

        return host.UseSerilog();
    }
}