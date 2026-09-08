using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Haptics.Filters;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics.Effects;

public sealed class ImpactEffect : IHapticEffect
{
    private readonly TransientEnvelope _envelope = new();
    private float _latBias;
    private float _lastMagnitude;

    public string Name => "Impact";

    public HapticSignal Update(VehicleTelemetry telemetry, HapticSettings settings, float deltaSeconds)
    {
        var thud = telemetry.SuspensionThud;
        var magnitude = Math.Max(telemetry.ImpactMagnitude, thud);
        var rising = magnitude > 0.04f && magnitude >= _lastMagnitude + 0.02f;
        if (rising)
        {
            var hold = Math.Max(settings.ImpactAttackMs, 70f) / 1000f;
            var intensity = Math.Clamp(settings.ImpactCurve.Evaluate(Math.Max(magnitude, 0.08f)) * settings.ImpactGain, 0f, 1f);
            _envelope.Trigger(intensity, hold);
            if (thud >= telemetry.ImpactMagnitude && thud > 0.04f)
            {
                var span = Math.Max(thud, 0.001f);
                _latBias = Math.Clamp((telemetry.SuspensionThudRight - telemetry.SuspensionThudLeft) / span * 0.4f, -0.4f, 0.4f);
            }
            else
            {
                _latBias = Math.Clamp(telemetry.AccelLateral / 20f, -0.4f, 0.4f);
            }
        }

        _lastMagnitude = Math.Max(magnitude, _lastMagnitude * MathF.Exp(-deltaSeconds / 0.08f));
        var decay = Math.Max(settings.ImpactDecayMs, 180f) / 1000f;
        var level = _envelope.Update(0.008f, decay, deltaSeconds);
        return new HapticSignal(level * (1f - _latBias * 0.25f), level * (1f + _latBias * 0.25f));
    }

    public void Reset()
    {
        _envelope.Reset();
        _latBias = 0f;
        _lastMagnitude = 0f;
    }
}
