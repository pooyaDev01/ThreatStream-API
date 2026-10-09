using FluentValidation;
using Microsoft.AspNetCore.Authentication.Negotiate;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Infrastructure.Configuration;
using ThreatStream.Infrastructure.Messaging;
using ThreatStream.Ingestion.Api.Configuration;
using ThreatStream.Ingestion.Api.Middleware;
using ThreatStream.Ingestion.Api.Services;
using ThreatStream.Ingestion.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));

builder.Services.Configure<MessagingOptions>(builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton<IMessagePublisher, KafkaMessagePublisher>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITenantContext, TenantContext>();

builder.Services.AddScoped<IEventIngestionService, EventIngestionService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

builder.Services.AddValidatorsFromAssemblyContaining<SecurityEventRequestValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
