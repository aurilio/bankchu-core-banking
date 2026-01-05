# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Copia apenas os projetos (não a solution)
COPY src/ src/

# Restore baseado no projeto da API
RUN dotnet restore src/BankChu.CoreBanking.Api/BankChu.CoreBanking.Api.csproj

# Publish
RUN dotnet publish src/BankChu.CoreBanking.Api/BankChu.CoreBanking.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BankChu.CoreBanking.Api.dll"]
