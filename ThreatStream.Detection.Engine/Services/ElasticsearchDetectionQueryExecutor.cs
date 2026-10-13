using System.Text;
using System.Text.Json;
using ThreatStream.Detection.Engine.Abstractions;
using ThreatStream.Detection.Engine.Configuration;
using ThreatStream.Detection.Engine.Models;
using Microsoft.Extensions.Options;

namespace ThreatStream.Detection.Engine.Services;

public sealed class ElasticsearchDetectionQueryExecutor: IDetectionQueryExecutor
{
    private readonly HttpClient _httpClient;
    private readonly DetectionEngineOptions _options;

    public ElasticsearchDetectionQueryExecutor(HttpClient httpClient, IOptions<DetectionEngineOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<string>> ExecuteAsync(DetectionRule rule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rule);

        if (rule.TenantId == Guid.Empty)
            throw new InvalidOperationException("Detection rule must have a tenant ID.");

        if (string.IsNullOrWhiteSpace(rule.ElasticQueryDsl))
            throw new InvalidOperationException($"Rule '{rule.Id}' has an empty Elasticsearch query.");

        using var ruleDocument = JsonDocument.Parse(rule.ElasticQueryDsl);

        if (!ruleDocument.RootElement.TryGetProperty("query", out var ruleQuery))
        {
            throw new InvalidOperationException($"Rule '{rule.Id}' must contain a top-level 'query' property.");
        }

        var requestBody = new
        {
            size = 1000,
            track_total_hits = true,
            query = new
            {
                @bool = new
                {
                    filter = new object[]
                    {
                        new
                        {
                            term = new Dictionary<string, string>
                            {
                                ["tenant_id"] = rule.TenantId.ToString()
                            }
                        },
                        ruleQuery
                    }
                }
            }
        };

        var url =$"{_options.IndexName}/_search";

        using var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        using var response = await _httpClient.PostAsync(url, content, cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Elasticsearch query failed with HTTP " + $"{(int)response.StatusCode}: {responseBody}");
        }

        using var result = JsonDocument.Parse(responseBody);

        var ids = new List<string>();

        if (!result.RootElement.TryGetProperty("hits", out var hits) || !hits.TryGetProperty("hits", out var hitItems))
        {
            throw new InvalidOperationException("Elasticsearch response did not contain hits.hits.");
        }

        foreach (var hit in hitItems.EnumerateArray())
        {
            if (hit.TryGetProperty("_id", out var id) && id.ValueKind == JsonValueKind.String)
            {
                ids.Add(id.GetString()!);
            }
        }

        return ids;
    }
}

