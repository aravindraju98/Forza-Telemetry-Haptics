using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics;

public sealed class HapticMixer
{
    public HapticSignal Mix(
        HapticSignal engine,
        HapticSignal road,
        HapticSignal slip,
        HapticSignal drift,
        HapticSignal gForce,
        HapticSignal abs,
        HapticSignal traction,
        HapticSignal handbrake,
        HapticSignal gear,
        HapticSignal impact,
        HapticSettings settings)
    {
        var baseline = engine + road;
        var dynamic = slip + drift + gForce;
        var pulses = abs + traction + handbrake;
        var layered = baseline + dynamic + pulses;
        var hits = new HapticSignal(
            Math.Max(gear.LeftMotor, impact.LeftMotor),
            Math.Max(gear.RightMotor, impact.RightMotor));

        var impactWeight = MathUtil.Clamp01(hits.Peak * settings.ImpactPriority);
        var leftover = 1f - impactWeight;
        var mixed = new HapticSignal(
            hits.LeftMotor + layered.LeftMotor * leftover,
            hits.RightMotor + layered.RightMotor * leftover);

        mixed *= settings.GlobalGain;
        var max = Math.Clamp(settings.MaximumRumble, 0.01f, 1f);
        var leftScale = Math.Clamp(settings.LeftMotorScale, 0.4f, 1.2f);
        var rightScale = Math.Clamp(settings.RightMotorScale, 0.4f, 1.2f);
        var left = Math.Clamp(mixed.LeftMotor * leftScale, 0f, max);
        var right = Math.Clamp(mixed.RightMotor * rightScale, 0f, max);

        if (left < settings.MinimumThreshold) left = 0f;
        if (right < settings.MinimumThreshold) right = 0f;
        return new HapticSignal(left, right);
    }
}
