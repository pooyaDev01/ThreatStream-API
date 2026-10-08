using System;
using System.Collections.Generic;
using System.Text;

namespace ThreatStream.Infrastructure.Configuration
{
    public class KafkaOptions
    {
        public string BootstrapServers { get; set; } = string.Empty;
    }
}
