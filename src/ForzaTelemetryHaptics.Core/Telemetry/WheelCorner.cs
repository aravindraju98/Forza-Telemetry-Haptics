namespace ForzaTelemetryHaptics.Telemetry;

/// <summary>Per-wheel values in FL, FR, RL, RR order.</summary>
public readonly record struct WheelCorner<T>(T FrontLeft, T FrontRight, T RearLeft, T RearRight)
{
    public T this[int index] => index switch
    {
        0 => FrontLeft,
        1 => FrontRight,
        2 => RearLeft,
        3 => RearRight,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };
}
