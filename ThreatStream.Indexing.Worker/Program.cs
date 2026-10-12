using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Indexing.Worker;
using ThreatStream.Indexing.Worker.Configuration;
using ThreatStream.Infrastructure.Configuration;
using ThreatStream.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.Services.Configure<KafkaOptions>(
    builder.Configuration.GetSection("Kafka"));

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

var host = builder.Build();
host.Run();
