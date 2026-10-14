using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Enrichment.Worker;
using ThreatStream.Enrichment.Worker.Configuration;
using ThreatStream.Enrichment.Worker.Services;
using ThreatStream.Infrastructure.Configuration;
using ThreatStream.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));

builder.Services.Configure<MessagingOptions>(builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton(builder.Configuration.GetSection("Kafka").Get<MessagingOptions>()!);

builder.Services.AddSingleton<IMessagePublisher, KafkaMessagePublisher>();

builder.Services.AddSingleton<RedisThreatIntelService>();

builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection("Redis"));

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