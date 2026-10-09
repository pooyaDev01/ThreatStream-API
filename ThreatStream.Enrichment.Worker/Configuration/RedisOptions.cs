using System;
using System.Collections.Generic;
using System.Text;

namespace ThreatStream.Enrichment.Worker.Configuration;

public class RedisOptions
{
    public string ConnectionString { get; set; } = string.Empty;

    public int CacheTtlHours { get; set; } = 24;
}
