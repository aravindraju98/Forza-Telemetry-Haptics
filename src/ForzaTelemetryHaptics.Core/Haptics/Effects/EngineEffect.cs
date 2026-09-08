using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Haptics.Filters;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics.Effects;

public sealed class EngineEffect : IHapticEffect
{
    private readonly LowPassFilter _filter = new();
    private float _time;

    public string Name => "Engine";

    public HapticSignal Update(VehicleTelemetry telemetry, HapticSettings settings, float deltaSeconds)
    {
        _time += deltaSeconds;
        if (!telemetry.EngineRunning)
        {
            var quiet = _filter.Update(0f, settings.ReleaseSeconds, deltaSeconds);
            return new HapticSignal(quiet, quiet * 0.92f);
        }

        var shaped = settings.EngineCurve.Evaluate(telemetry.NormalizedRpm);
        var throttleBlend = Math.Clamp(settings.EngineThrottleBlend, 0f, 1f);
        var throttleScale = 1f - throttleBlend + throttleBlend * MathUtil.Clamp01(telemetry.Throttle);
        var curve = settings.EngineBase + settings.EngineGain * shaped * throttleScale;
        var freq = settings.EngineModulationHz +
                   (settings.EngineRedlineModHz - settings.EngineModulationHz) * MathF.Pow(telemetry.NormalizedRpm, 2f);
        var modulation = 1f + settings.EngineModulation * 0.5f * MathF.Sin(_time * freq * MathF.Tau);
        var intensity = _filter.Update(curve * modulation, settings.SmoothingSeconds, deltaSeconds);
        intensity = Math.Min(intensity, 0.55f);
        return new HapticSignal(intensity, intensity * 0.94f);
    }

    public void Reset()
    {
        _filter.Reset();
        _time = 0f;
    }
}
