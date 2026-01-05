# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

COPY BankChu.CoreBanking.sln .
COPY src/BankChu.CoreBanking.Api/BankChu.CoreBanking.Api.csproj src/BankChu.CoreBanking.Api/
COPY src/BankChu.CoreBanking.Application/BankChu.CoreBanking.Application.csproj src/BankChu.CoreBanking.Application/
COPY src/BankChu.CoreBanking.Domain/BankChu.CoreBanking.Domain.csproj src/BankChu.CoreBanking.Domain/
COPY src/BankChu.CoreBanking.Infrastructure/BankChu.CoreBanking.Infrastructure.csproj src/BankChu.CoreBanking.Infrastructure/

RUN dotnet restore

COPY src/ src/
RUN dotnet publish src/BankChu.CoreBanking.Api/BankChu.CoreBanking.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BankChu.CoreBanking.Api.dll"]
