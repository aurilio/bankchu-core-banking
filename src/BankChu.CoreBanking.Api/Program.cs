using BankChu.CoreBanking.Api.Endpoints;
using BankChu.CoreBanking.Api.Endpoints.Accounts;
using BankChu.CoreBanking.Api.Extensions;
using BankChu.CoreBanking.IoC;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddCoreBanking(builder.Configuration);
builder.Services.AddScoped<StatementQueryHandler>();

var app = builder.Build();

app.UseGlobalExceptionHandler();

app.ApplyMigrations();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

app.MapApiEndpoints();

app.Run();