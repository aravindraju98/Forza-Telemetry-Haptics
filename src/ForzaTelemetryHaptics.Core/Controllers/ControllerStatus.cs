namespace ForzaTelemetryHaptics.Controllers;

public sealed class ControllerStatus
{
    public bool Detected { get; init; }
    public string ControllerType { get; init; } = "None";
    public int Index { get; init; } = -1;
    public bool RumbleAvailable { get; init; }
    public string DisplayName { get; init; } = "No controller";
    public string Error { get; init; } = string.Empty;
}
