namespace ThreatStream.Detection.Engine.Configuration;

public sealed class DetectionEngineOptions
{
    public string ElasticsearchUrl { get; set; } = "http://localhost:9200";

    public string IndexName { get; set; } = "security-events";

    public string ConnectionString { get; set; } = string.Empty;

    public int SearchRetryCount { get; set; } = 3;

    public int SearchRetryDelayMilliseconds { get; set; } = 1000;
}
