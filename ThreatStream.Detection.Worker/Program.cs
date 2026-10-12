using ThreatStream.Contracts.Messaging;
using ThreatStream.Detection.Worker;
using ThreatStream.Infrastructure.Configuration;
using ThreatStream.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaOptions>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.Configure<MessagingOptions>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.AddHostedService<Worker>();

builder.Services.AddSingleton<IMessageConsumer, KafkaMessageConsumer>();

var host = builder.Build();
host.Run();
