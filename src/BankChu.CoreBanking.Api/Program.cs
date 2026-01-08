using BankChu.CoreBanking.Api.Auth;
using BankChu.CoreBanking.Api.Endpoints;
using BankChu.CoreBanking.Api.Endpoints.Accounts;
using BankChu.CoreBanking.Api.Extensions;
using BankChu.CoreBanking.IoC;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Core (Domain + Application + Infra)
builder.Services.AddCoreBanking(builder.Configuration);

// API concerns
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();
builder.Services.AddSwaggerWithAuth();

// API-only handlers
builder.Services.AddScoped<StatementQueryHandler>();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.ApplyMigrations();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

app.UseAuthentication();
app.UseAuthorization();

app.MapApiEndpoints();

app.Run();