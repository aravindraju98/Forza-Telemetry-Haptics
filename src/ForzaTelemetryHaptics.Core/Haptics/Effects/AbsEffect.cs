using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Haptics.Filters;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics.Effects;

public sealed class AbsEffect : IHapticEffect
{
    private readonly PulseGenerator _pulse = new();

    public string Name => "ABS";

    public HapticSignal Update(VehicleTelemetry telemetry, HapticSettings settings, float deltaSeconds)
    {
        var pulse = _pulse.Update(telemetry.AbsActive, settings.AbsFrequencyHz, settings.AbsPulseWidth, deltaSeconds);
        var intensity = pulse * settings.AbsCurve.Evaluate(telemetry.AbsStrength) * settings.AbsGain;
        var left = intensity * (0.7f + 0.3f * Math.Max(telemetry.FrontLeftSlip, telemetry.RearLeftSlip));
        var right = intensity * (0.7f + 0.3f * Math.Max(telemetry.FrontRightSlip, telemetry.RearRightSlip));
        return new HapticSignal(left, right);
    }

    public void Reset() => _pulse.Reset();
}
