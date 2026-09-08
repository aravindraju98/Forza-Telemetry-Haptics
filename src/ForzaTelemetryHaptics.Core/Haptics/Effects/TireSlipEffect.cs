using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Haptics.Filters;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics.Effects;

public sealed class TireSlipEffect : IHapticEffect
{
    private readonly LowPassFilter _left = new();
    private readonly LowPassFilter _right = new();

    public string Name => "Slip";

    public HapticSignal Update(VehicleTelemetry telemetry, HapticSettings settings, float deltaSeconds)
    {
        if (!telemetry.IsDriving)
        {
            return new HapticSignal(
                _left.Update(0f, settings.ReleaseSeconds, deltaSeconds),
                _right.Update(0f, settings.ReleaseSeconds, deltaSeconds));
        }

        var fl = settings.SlipCurve.Evaluate(telemetry.FrontLeftSlip);
        var fr = settings.SlipCurve.Evaluate(telemetry.FrontRightSlip);
        var rl = settings.SlipCurve.Evaluate(telemetry.RearLeftSlip);
        var rr = settings.SlipCurve.Evaluate(telemetry.RearRightSlip);

        var spin = TelemetryNormalizer.DrivenWheelSpin(telemetry.DrivetrainType, telemetry.SlipRatio);
        var wheelspin = MathUtil.Clamp01(spin) * telemetry.Throttle * settings.WheelspinGain;

        // Front slip biases the matching motor; rear slip uses the opposite pair for a "from behind" feel.
        var left = (fl * 0.70f + fr * 0.15f + rl * 0.20f + rr * 0.45f) * settings.SlipGain + wheelspin;
        var right = (fr * 0.70f + fl * 0.15f + rr * 0.20f + rl * 0.45f) * settings.SlipGain + wheelspin;

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
