using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Haptics.Filters;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics.Effects;

public sealed class GForceEffect : IHapticEffect
{
    private readonly LowPassFilter _left = new();
    private readonly LowPassFilter _right = new();

    public string Name => "G-force";

    public HapticSignal Update(VehicleTelemetry telemetry, HapticSettings settings, float deltaSeconds)
    {
        if (!settings.GForceEnabled || !telemetry.IsDriving)
        {
            return new HapticSignal(
                _left.Update(0f, settings.ReleaseSeconds, deltaSeconds),
                _right.Update(0f, settings.ReleaseSeconds, deltaSeconds));
        }

        var accel = MathUtil.Clamp01(telemetry.AccelLongitudinal / 12f) * 0.55f;
        var brake = MathUtil.Clamp01(-telemetry.AccelLongitudinal / 14f) * 0.70f;
        var lat = Math.Clamp(telemetry.AccelLateral / 16f, -1f, 1f);
        var shaped = settings.GForceCurve.Evaluate(MathUtil.Clamp01(accel + brake));
        var body = shaped * settings.GForceGain;
        var left = body * (1f + Math.Max(0f, -lat) * 0.6f) + Math.Max(0f, -lat) * settings.GForceGain * 0.35f;
        var right = body * (1f + Math.Max(0f, lat) * 0.6f) + Math.Max(0f, lat) * settings.GForceGain * 0.35f;

        return new HapticSignal(
            _left.Update(left, settings.SmoothingSeconds, deltaSeconds),
            _right.Update(right, settings.SmoothingSeconds, deltaSeconds));
    }

    public void Reset()
    {
        _left.Reset();
        _right.Reset();
    }
}
