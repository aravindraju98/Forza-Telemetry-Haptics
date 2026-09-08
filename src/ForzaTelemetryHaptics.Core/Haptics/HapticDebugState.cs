namespace ForzaTelemetryHaptics.Haptics;

public sealed record HapticDebugState
{
    public float Engine { get; init; }
    public float Boost { get; init; }
    public float Road { get; init; }
    public float Slip { get; init; }
    public float Drift { get; init; }
    public float Abs { get; init; }
    public float Traction { get; init; }
    public float Handbrake { get; init; }
    public float GearShift { get; init; }
    public float Impact { get; init; }
    public float GForce { get; init; }
    public float LeftMotor { get; init; }
    public float RightMotor { get; init; }
    public bool FadingOut { get; init; }
}
