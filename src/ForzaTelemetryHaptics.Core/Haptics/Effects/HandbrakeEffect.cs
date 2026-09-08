using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Haptics.Filters;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics.Effects;

public sealed class HandbrakeEffect : IHapticEffect
{
    private readonly LowPassFilter _filter = new();

    public string Name => "Handbrake";

    public HapticSignal Update(VehicleTelemetry telemetry, HapticSettings settings, float deltaSeconds)
    {
        var held = telemetry.IsDriving && telemetry.Handbrake > 0.12f;
        // Silent when parked; full grab around 45 km/h so idle e-brake does not buzz.
        var speedScale = MathUtil.Clamp01((telemetry.SpeedMps - 0.8f) / 12f);
        var raw = held
            ? settings.HandbrakeGain * (0.55f + 0.45f * telemetry.Handbrake) * speedScale
            : 0f;
        var intensity = _filter.Update(raw, held && speedScale > 0f ? settings.AttackSeconds : settings.ReleaseSeconds, deltaSeconds);
        return new HapticSignal(intensity, intensity * 0.92f);
    }

    public void Reset() => _filter.Reset();
}
