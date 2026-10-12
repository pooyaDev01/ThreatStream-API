using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Options;
using ThreatStream.Contracts.Messaging;
using ThreatStream.Indexing.Worker.Configuration;

namespace ThreatStream.Indexing.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMessageConsumer _messageConsumer;
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly ElasticsearchOptions _elasticsearchOptions;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IMessageConsumer messageConsumer, ElasticsearchClient elasticsearchClient, ElasticsearchOptions elasticsearchOptions, IConfiguration configuration)
        {
            _logger = logger;
            _messageConsumer = messageConsumer;
            _elasticsearchClient = elasticsearchClient;
            _elasticsearchOptions = elasticsearchOptions;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

        }
    }
}
