using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Haptics.Filters;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics.Effects;

public sealed class DriftEffect : IHapticEffect
{
    private readonly LowPassFilter _filter = new();

    public string Name => "Drift";

    public HapticSignal Update(VehicleTelemetry telemetry, HapticSettings settings, float deltaSeconds)
    {
        var rearAngle = Math.Max(MathF.Abs(telemetry.SlipAngle.RearLeft), MathF.Abs(telemetry.SlipAngle.RearRight));
        var magnitude = Math.Max(telemetry.RearSlip, MathUtil.Clamp01(rearAngle));
        var lateral = MathUtil.Clamp01(MathF.Abs(telemetry.AccelLateral) / 18f);
        var raw = settings.DriftCurve.Evaluate(magnitude) * (0.65f + 0.35f * lateral) * settings.DriftGain;

        var intensity = _filter.Update(raw, settings.SmoothingSeconds, deltaSeconds);
        var bias = Math.Clamp(telemetry.AccelLateral / 14f, -0.35f, 0.35f);
        return new HapticSignal(intensity * (1f - bias), intensity * (1f + bias));
    }

    public void Reset() => _filter.Reset();
}
