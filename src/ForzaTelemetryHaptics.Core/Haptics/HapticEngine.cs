using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Haptics.Effects;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics;

public sealed class HapticEngine
{
    private readonly HapticMixer _mixer = new();
    private readonly EngineEffect _engine = new();
    private readonly BoostEffect _boost = new();
    private readonly RoadEffect _road = new();
    private readonly TireSlipEffect _slip = new();
    private readonly DriftEffect _drift = new();
    private readonly AbsEffect _abs = new();
    private readonly TractionEffect _traction = new();
    private readonly HandbrakeEffect _handbrake = new();
    private readonly GearShiftEffect _gear = new();
    private readonly ImpactEffect _impact = new();
    private readonly GForceEffect _gForce = new();
    private float _outputLeft;
    private float _outputRight;

    public HapticDebugState LastDebug { get; private set; } = new();

    public HapticSignal Update(VehicleTelemetry? telemetry, HapticSettings settings, float deltaSeconds, bool live)
    {
        deltaSeconds = Math.Clamp(deltaSeconds, 0.001f, 0.1f);
        if (!live || telemetry is null || !IsActive(telemetry))
        {
            return FadeOut(settings, deltaSeconds);
        }
        var engine = Gate(settings.EngineEnabled, _engine.Update(telemetry, settings, deltaSeconds));
        var boost = Gate(settings.BoostEnabled, _boost.Update(telemetry, settings, deltaSeconds));
        var road = Gate(settings.RoadEnabled, _road.Update(telemetry, settings, deltaSeconds));
        var slip = Gate(settings.SlipEnabled, _slip.Update(telemetry, settings, deltaSeconds));
        var drift = Gate(settings.DriftEnabled, _drift.Update(telemetry, settings, deltaSeconds));
        var abs = Gate(settings.AbsEnabled, _abs.Update(telemetry, settings, deltaSeconds));
        var traction = Gate(settings.TractionEnabled, _traction.Update(telemetry, settings, deltaSeconds));
        var handbrake = Gate(settings.HandbrakeEnabled, _handbrake.Update(telemetry, settings, deltaSeconds));
        var gear = Gate(settings.GearEnabled, _gear.Update(telemetry, settings, deltaSeconds));
        var impact = Gate(settings.ImpactEnabled, _impact.Update(telemetry, settings, deltaSeconds));
        var gForce = Gate(settings.GForceEnabled, _gForce.Update(telemetry, settings, deltaSeconds));

        var mixed = _mixer.Mix(engine + boost, road, slip, drift, gForce, abs, traction, handbrake, gear, impact, settings);
        _outputLeft = mixed.LeftMotor;
        _outputRight = mixed.RightMotor;
        LastDebug = new HapticDebugState
        {
            Engine = engine.Peak,
            Boost = boost.Peak,
            Road = road.Peak,
            Slip = slip.Peak,
            Drift = drift.Peak,
            Abs = abs.Peak,
            Traction = traction.Peak,
            Handbrake = handbrake.Peak,
            GearShift = gear.Peak,
            Impact = impact.Peak,
            GForce = gForce.Peak,
            LeftMotor = mixed.LeftMotor,
            RightMotor = mixed.RightMotor
        };
        return mixed;
    }

    public void Reset()
    {
        _engine.Reset();
        _boost.Reset();
        _road.Reset();
        _slip.Reset();
        _drift.Reset();
        _abs.Reset();
        _traction.Reset();
        _handbrake.Reset();
        _gear.Reset();
        _impact.Reset();
        _gForce.Reset();
        _outputLeft = 0f;
        _outputRight = 0f;
        LastDebug = new HapticDebugState();
    }

    private HapticSignal FadeOut(HapticSettings settings, float deltaSeconds)
    {
        var tau = Math.Max(settings.FadeOutSeconds, 0.05f);
        var alpha = 1f - MathF.Exp(-deltaSeconds / tau);
        _outputLeft += (0f - _outputLeft) * alpha;
        _outputRight += (0f - _outputRight) * alpha;
        if (_outputLeft < 0.002f) _outputLeft = 0f;
        if (_outputRight < 0.002f) _outputRight = 0f;

        // Do not Reset() here: Horizon can flicker IsRaceOn and wiping gear
        // history would swallow the next shift.

        LastDebug = new HapticDebugState
        {
            LeftMotor = _outputLeft,
            RightMotor = _outputRight,
            FadingOut = true
        };
        return new HapticSignal(_outputLeft, _outputRight);
    }

    private static HapticSignal Gate(bool enabled, HapticSignal signal) =>
        enabled ? signal : HapticSignal.Zero;

    private static bool IsActive(VehicleTelemetry telemetry) =>
        telemetry.IsDriving || telemetry.EngineRunning || telemetry.Rpm > 200f || telemetry.SpeedMps > 0.35f;
}
