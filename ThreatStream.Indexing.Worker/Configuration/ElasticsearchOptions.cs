using System;
using System.Collections.Generic;
using System.Text;

namespace ThreatStream.Indexing.Worker.Configuration;

public class ElasticsearchOptions
{
    public string Url { get; set; } = "http://localhost:9200";

    public string IndexName { get; set; } = "security-events";
}
