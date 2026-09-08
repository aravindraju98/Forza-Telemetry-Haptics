using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Haptics.Filters;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics.Effects;

public sealed class RoadEffect : IHapticEffect
{
    private readonly LowPassFilter _left = new();
    private readonly LowPassFilter _right = new();

    public string Name => "Road";

    public HapticSignal Update(VehicleTelemetry telemetry, HapticSettings settings, float deltaSeconds)
    {
        if (!telemetry.IsDriving || telemetry.SpeedMps < 0.6f)
        {
            return new HapticSignal(
                _left.Update(0f, settings.ReleaseSeconds, deltaSeconds),
                _right.Update(0f, settings.ReleaseSeconds, deltaSeconds));
        }

        var speedFactor = settings.RoadCurve.Evaluate(telemetry.NormalizedSpeed);
        var leftSurface = MathF.Max(telemetry.SurfaceRumble.FrontLeft, telemetry.SurfaceRumble.RearLeft);
        var rightSurface = MathF.Max(telemetry.SurfaceRumble.FrontRight, telemetry.SurfaceRumble.RearRight);
        var leftStrip = (telemetry.OnRumbleStrip.FrontLeft ? 0.55f : 0f)
                        + (telemetry.OnRumbleStrip.RearLeft ? 0.45f : 0f);
        var rightStrip = (telemetry.OnRumbleStrip.FrontRight ? 0.55f : 0f)
                         + (telemetry.OnRumbleStrip.RearRight ? 0.45f : 0f);

        var bed = speedFactor * settings.RoadGain;
        var left = bed * 0.96f
                   + speedFactor * leftSurface * settings.RoadSurfaceGain
                   + leftStrip * settings.RoadRumbleStripGain
                   + telemetry.PuddleLeft * settings.RoadPuddleGain;
        var right = bed
                    + speedFactor * rightSurface * settings.RoadSurfaceGain
                    + rightStrip * settings.RoadRumbleStripGain
                    + telemetry.PuddleRight * settings.RoadPuddleGain;

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
