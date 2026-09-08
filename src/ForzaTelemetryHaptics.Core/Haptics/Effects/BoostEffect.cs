using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Haptics.Filters;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics.Effects;

public sealed class BoostEffect : IHapticEffect
{
    private readonly LowPassFilter _filter = new();

    public string Name => "Boost";

    public HapticSignal Update(VehicleTelemetry telemetry, HapticSettings settings, float deltaSeconds)
    {
        if (!telemetry.EngineRunning || !telemetry.BoostActive)
        {
            var quiet = _filter.Update(0f, settings.ReleaseSeconds, deltaSeconds);
            return new HapticSignal(quiet * 0.85f, quiet);
        }

        var shaped = settings.BoostCurve.Evaluate(telemetry.NormalizedBoost);
        var intensity = _filter.Update(
            shaped * settings.BoostGain,
            settings.SmoothingSeconds,
            deltaSeconds);
        return new HapticSignal(intensity * 0.85f, intensity);
    }

    public void Reset() => _filter.Reset();
}
