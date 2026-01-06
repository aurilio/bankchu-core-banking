using BankChu.CoreBanking.Api.Extensions;
using BankChu.CoreBanking.Infrastructure.Persistence;
using BankChu.CoreBanking.IoC;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddCoreBanking(builder.Configuration);

var app = builder.Build();

// Migration automática (bootstrap)
app.ApplyMigrations();

// Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Health endpoint
app.MapHealthChecks("/health");

app.Run();