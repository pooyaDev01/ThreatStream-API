namespace ThreatStream.Infrastructure.Configuration;

public class MessagingOptions
{
    public string SecurityEventsTopic { get; set; } = string.Empty;

    public string EnrichedMessagesTopic { get; set; } = string.Empty;
}