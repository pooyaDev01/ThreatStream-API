
using ThreatStream.Detection.Engine.Models;

namespace ThreatStream.Detection.Engine.Abstractions;

public interface IDetectionQueryExecutor
{
    Task<IReadOnlyList<string>> ExecuteAsync(DetectionRule rule, CancellationToken cancellationToken = default);
}
