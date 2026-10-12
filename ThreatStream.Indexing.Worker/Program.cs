using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Indexing.Worker;
using ThreatStream.Indexing.Worker.Configuration;
using ThreatStream.Infrastructure.Configuration;
using ThreatStream.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<KafkaOptions>(
    builder.Configuration.GetSection("Kafka"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<MessagingOptions>>().Value);

builder.Services.Configure<ElasticsearchOptions>(
    builder.Configuration.GetSection("Elasticsearch"));

builder.Services.AddSingleton(sp =>
{
    var options = sp
        .GetRequiredService<IOptions<ElasticsearchOptions>>()
        .Value;

    var settings = new ElasticsearchClientSettings(
        new Uri(options.Url));

    return new ElasticsearchClient(settings);
});

builder.Services.AddSingleton<IMessageConsumer, KafkaMessageConsumer>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();