using BankChu.CoreBanking.Api.Endpoints;
using BankChu.CoreBanking.Api.Endpoints.Accounts;
using BankChu.CoreBanking.Api.Extensions;
using BankChu.CoreBanking.Api.Middlewares;
using BankChu.CoreBanking.IoC;


var builder = WebApplication.CreateBuilder(args);

builder.Host.AddSerilogLogging(builder.Configuration);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.AddCoreBanking(builder.Configuration);

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorizationPolicies();
builder.Services.AddSwaggerWithAuth();
builder.Services.AddApiJsonSerialization();

builder.Services.AddScoped<StatementQueryHandler>();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseObservability();

app.ApplyMigrations();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");

app.UseGlobalExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapApiEndpoints();

app.Run();