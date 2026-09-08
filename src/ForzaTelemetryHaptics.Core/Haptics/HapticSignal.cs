using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics;

public readonly record struct HapticSignal(float LeftMotor, float RightMotor)
{
    public static HapticSignal Zero => new(0f, 0f);

    public HapticSignal Clamp() => new(MathUtil.Clamp01(LeftMotor), MathUtil.Clamp01(RightMotor));

    public static HapticSignal operator +(HapticSignal a, HapticSignal b) =>
        new(a.LeftMotor + b.LeftMotor, a.RightMotor + b.RightMotor);

    public static HapticSignal operator *(HapticSignal a, float gain) =>
        new(a.LeftMotor * gain, a.RightMotor * gain);

    public float Peak => MathF.Max(LeftMotor, RightMotor);
}
