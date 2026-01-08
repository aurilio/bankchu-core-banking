using BankChu.CoreBanking.Application.Abstractions.Persistence;
using BankChu.CoreBanking.Application.Abstractions.Services;
using BankChu.CoreBanking.Application.Accounts.Create;
using BankChu.CoreBanking.Application.Transfers;
using BankChu.CoreBanking.Infrastructure.External;
using BankChu.CoreBanking.Infrastructure.Persistence;
using BankChu.CoreBanking.Infrastructure.Persistence.Repositories;
using BankChu.CoreBanking.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Refit;
using StackExchange.Redis;


namespace BankChu.CoreBanking.IoC;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreBanking(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        AddDatabase(services, configuration);
        AddCache(services, configuration);
        AddRedisMultiplexer(services, configuration);
        AddExternalClients(services);
        AddRepositories(services);
        AddUnitOfWork(services);
        AddServices(services);
        AddValidators(services);

        return services;
    }

    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CoreBankingDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(CoreBankingDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                });
        });
    }

    private static void AddCache(IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "BankChu:";
        });
    }

    private static void AddRedisMultiplexer(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var connectionString = configuration.GetConnectionString("Redis");

            return ConnectionMultiplexer.Connect(connectionString);
        });
    }

    private static void AddExternalClients(IServiceCollection services)
    {
        services.AddRefitClient<IBrasilApiClient>()
            .ConfigureHttpClient((serviceProvider, httpClient) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                httpClient.BaseAddress = new Uri(configuration["ExternalApis:BrasilApi:BaseUrl"]!);

                httpClient.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("ExternalApis:BrasilApi:TimeoutSeconds"));
            })
            .AddPolicyHandler(
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(
                        retryCount: 3,
                        sleepDurationProvider: retry =>
                            TimeSpan.FromSeconds(Math.Pow(2, retry))
                    )
            );
    }


    private static void AddRepositories(IServiceCollection services)
    {
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransferRepository, TransferRepository>();
    }

    private static void AddUnitOfWork(IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<CreateAccountService>();
        services.AddScoped<TransferFundsService>();
        services.AddScoped<IBusinessDayService, BusinessDayService>();
        services.AddScoped<IIdempotencyService, RedisIdempotencyService>();
    }

    private static void AddValidators(IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(CreateAccountValidator).Assembly);
    }
}