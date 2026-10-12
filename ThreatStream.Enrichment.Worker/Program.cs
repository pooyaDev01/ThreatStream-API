using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Enrichment.Worker;
using ThreatStream.Enrichment.Worker.Configuration;
using ThreatStream.Infrastructure.Configuration;
using ThreatStream.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaOptions>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<MessagingOptions>>().Value);

builder.Services.Configure<RedisOptions>(
    builder.Configuration.GetSection("Redis"));

builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
{
    var options = serviceProvider
        .GetRequiredService<IOptions<RedisOptions>>()
        .Value;

    return ConnectionMultiplexer.Connect(options.ConnectionString);
});

builder.Services.AddSingleton<IMessageConsumer, KafkaMessageConsumer>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
