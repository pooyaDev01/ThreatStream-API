using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThreatStream.Ingestion.Api.Models
{
    public class SecurityEventRequest
    {
        [JsonPropertyName("timestamp")]
        [Required]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("agent_id")]
        [Required]
        public string AgentId { get; set; } = string.Empty;

        [JsonPropertyName("event_type")]
        [Required]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("process_name")]
        [Required]
        public string ProcessName { get; set; } = string.Empty;

        [JsonPropertyName("file_hash")]
        [Required]
        public string FileHash { get; set; } = string.Empty;

        [JsonPropertyName("user")]
        [Required]
        public string User { get; set; } = string.Empty;

        [JsonPropertyName("ip_address")]
        [Required]
        public string IpAddress { get; set; } = string.Empty;
    }
}
