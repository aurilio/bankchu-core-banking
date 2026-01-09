using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;

namespace BankChu.CoreBanking.Api.Extensions;

public static class JsonSerializationExtensions
{
    public static IServiceCollection AddApiJsonSerialization(this IServiceCollection services)
    {
        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}