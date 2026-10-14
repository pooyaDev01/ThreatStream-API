using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;

namespace ThreatStream.Indexing.Worker.Setup;

public static class ElasticsearchIndexSetup
{
    public static async Task EnsureIndexAsync(ElasticsearchClient client, string indexName, CancellationToken cancellationToken = default)
    {
        var existsResponse = await client.Indices.ExistsAsync(indexName, cancellationToken);

        if (existsResponse.Exists)
            return;

        var request = new CreateIndexRequest(indexName)
        {
            Mappings = new TypeMapping
            {
                Properties = new Properties
                {
                    { "timestamp", new DateProperty() },
                    { "tenant_id", new KeywordProperty() },
                    { "agent_id", new KeywordProperty() },
                    { "event_type", new KeywordProperty() },
                    { "process_name", new KeywordProperty() },
                    { "file_hash", new KeywordProperty() },
                    { "user", new KeywordProperty() },
                    { "ip_address", new IpProperty() },
                    { "is_malicious", new KeywordProperty() },
                    { "threat_name", new KeywordProperty() },
                    { "severity", new KeywordProperty() }
                }
            }
        };

        var response = await client.Indices.CreateAsync(request, cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to create Elasticsearch index '{indexName}': " + response.DebugInformation);
        }
    }
}