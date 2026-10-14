using FluentValidation;
using Microsoft.Extensions.Options;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Infrastructure.Configuration;
using ThreatStream.Infrastructure.Messaging;
using ThreatStream.Ingestion.Api.Middleware;
using ThreatStream.Ingestion.Api.Services;
using ThreatStream.Ingestion.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));

builder.Services.Configure<MessagingOptions>(builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton<IMessagePublisher, KafkaMessagePublisher>();

builder.Services.AddScoped<IEventIngestionService, EventIngestionService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();

builder.Services.AddValidatorsFromAssemblyContaining<SecurityEventRequestValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();