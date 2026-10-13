using Microsoft.Extensions.Options;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Detection.Engine;
using ThreatStream.Detection.Engine.Configuration;
using ThreatStream.Detection.Worker;
using ThreatStream.Infrastructure.Configuration;
using ThreatStream.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaOptions>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.Configure<MessagingOptions>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<MessagingOptions>>().Value);

builder.Services.AddSingleton<IMessageConsumer, KafkaMessageConsumer>();

builder.Services.AddDetectionEngine(builder.Configuration);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
