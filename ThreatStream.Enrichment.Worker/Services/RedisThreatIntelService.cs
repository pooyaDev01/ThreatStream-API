
using System.Text.Json;
using StackExchange.Redis;
using ThreatStream.Enrichment.Worker.Models;

namespace ThreatStream.Enrichment.Worker.Services;

public class RedisThreatIntelService
{
    private readonly IDatabase _database;

    private static readonly JsonSerializerOptions JsonOptions = new() {

            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

    public RedisThreatIntelService(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<ThreatIntelData?> GetByHashAsync(string fileHash, CancellationToken ct)
    {
        var key = $"threat_intel:hash:{fileHash}";

        var cachedValue = await _database.StringGetAsync(key);

        if (cachedValue.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<ThreatIntelData>(
            cachedValue.ToString(),
            JsonOptions);
    }
}
