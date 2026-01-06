FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY src/ src/
RUN dotnet restore src/BankChu.CoreBanking.Api/BankChu.CoreBanking.Api.csproj

RUN dotnet publish src/BankChu.CoreBanking.Api/BankChu.CoreBanking.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

COPY --from=build /app/publish .

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s \
 CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "BankChu.CoreBanking.Api.dll"]
