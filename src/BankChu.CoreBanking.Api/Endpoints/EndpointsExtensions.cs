using BankChu.CoreBanking.Api.Endpoints.Accounts;
using BankChu.CoreBanking.Api.Endpoints.Auth;
using BankChu.CoreBanking.Api.Endpoints.Transfers;

namespace BankChu.CoreBanking.Api.Endpoints;

public static class EndpointsExtensions
{
    public static IEndpointRouteBuilder MapApiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAccountsEndpoints();
        endpoints.MapTransfersEndpoints();
        endpoints.MapGetStatement();
        endpoints.MapAuthEndpoints();

        return endpoints;
    }
}