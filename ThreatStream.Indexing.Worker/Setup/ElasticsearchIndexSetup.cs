using Elastic.Clients.Elasticsearch;

namespace ThreatStream.Indexing.Worker.Setup;

public static class ElasticsearchIndexSetup
{
    public static async Task EnsureIndexAsync(ElasticsearchClient client, string indexName, CancellationToken cancellationToken = default)
    {
        var existsResponse = await client.Indices.ExistsAsync(indexName, cancellationToken);

        if (existsResponse.Exists)
            return;

        var response = await client.Indices.CreateAsync(
            indexName,
            c => c
                .Mappings(m => m
                    .Properties(p => p
                        .Date("timestamp")
                        .Keyword("tenant_id")
                        .Keyword("agent_id")
                        .Keyword("event_type")
                        .Keyword("process_name")
                        .Keyword("file_hash")
                        .Keyword("user")
                        .Ip("ip_address")
                        .Keyword("is_malicious")
                        .Keyword("threat_name")
                        .Keyword("severity")
                    )
                ),
            cancellationToken);

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Failed to create Elasticsearch index '{indexName}': " + response.DebugInformation);
        }
    }
}
