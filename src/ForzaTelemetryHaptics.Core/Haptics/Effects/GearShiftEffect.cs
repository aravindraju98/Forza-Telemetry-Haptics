using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Haptics.Filters;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics.Effects;

public sealed class GearShiftEffect : IHapticEffect
{
    private readonly TransientEnvelope _envelope = new();
    private int? _previousGear;
    private float _sinceChange;

    public string Name => "Gear shift";
    public bool LastWasDownshift { get; private set; }

    public HapticSignal Update(VehicleTelemetry telemetry, HapticSettings settings, float deltaSeconds)
    {
        _sinceChange += deltaSeconds;
        var hold = Math.Max(settings.GearShiftDurationMs, 140f) / 1000f;
        var gearChanged = _previousGear is int previous
                          && telemetry.Gear != previous
                          && _sinceChange > 0.04f
                          && IsRealGearStep(previous, telemetry.Gear);

        if (_previousGear is null)
        {
            _previousGear = telemetry.Gear;
        }
        else if (gearChanged || (telemetry.GearShiftHint && _sinceChange > 0.18f))
        {
            LastWasDownshift = _previousGear is int prior && telemetry.Gear < prior && telemetry.Gear != 0;
            var scale = LastWasDownshift ? settings.GearShiftDownshiftScale : 1f;
            var amount = LastWasDownshift ? 1f : 0.7f;
            _envelope.Trigger(settings.GearCurve.Evaluate(amount) * settings.GearShiftGain * scale, hold);
            _previousGear = telemetry.Gear;
            _sinceChange = 0f;
        }
        else
        {
            _previousGear = telemetry.Gear;
        }

        var level = _envelope.Update(0.008f, hold, deltaSeconds);
        return new HapticSignal(level, level);
    }

    private static bool IsRealGearStep(int from, int to)
    {
        if (from == to)
        {
            return false;
        }

        // Ignore noise between reverse/neutral encodings.
        if ((from == 0 && to >= 11) || (from >= 11 && to == 0))
        {
            return false;
        }

        return true;
    }

    public void Reset()
    {
        _envelope.Reset();
        _previousGear = null;
        _sinceChange = 0f;
    }
}
