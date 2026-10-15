namespace ThreatStream.Detection.Engine.Models;

public static class AlertStatus
{
    public const string New = "New";
    public const string InProgress = "InProgress";
    public const string Resolved = "Resolved";
    public const string FalsePositive = "FalsePositive";

    public static bool IsValid(string status)
    {
        return (status == New || status == InProgress || status == Resolved || status == FalsePositive);
    }
}
