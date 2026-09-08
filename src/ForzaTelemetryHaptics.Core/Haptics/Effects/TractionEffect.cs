using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Haptics.Filters;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics.Effects;

public sealed class TractionEffect : IHapticEffect
{
    private readonly PulseGenerator _pulse = new();

    public string Name => "Traction";

    public HapticSignal Update(VehicleTelemetry telemetry, HapticSettings settings, float deltaSeconds)
    {
        var pulse = _pulse.Update(telemetry.TractionActive, settings.TractionFrequencyHz, 0.45f, deltaSeconds);
        var intensity = pulse * settings.TractionCurve.Evaluate(telemetry.TractionStrength) * settings.TractionGain;
        return new HapticSignal(intensity * 0.9f, intensity);
    }

    public void Reset() => _pulse.Reset();
}
