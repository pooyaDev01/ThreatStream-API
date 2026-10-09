using ThreatStream.Contracts.Messaging;
using ThreatStream.Enrichment.Worker;
using ThreatStream.Enrichment.Worker.Configuration;
using ThreatStream.Infrastructure.Configuration;
using ThreatStream.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaOptions>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.Configure<RedisOptions>(
    builder.Configuration.GetSection("Redis"));

builder.Services.AddSingleton<IMessageConsumer, KafkaMessageConsumer>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
