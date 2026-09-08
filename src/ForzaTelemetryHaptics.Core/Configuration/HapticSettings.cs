using ForzaTelemetryHaptics.Haptics;

namespace ForzaTelemetryHaptics.Configuration;

public sealed class HapticSettings
{
    public float GlobalGain { get; set; } = 1.0f;
    public float MaximumRumble { get; set; } = 0.35f;
    public float MinimumThreshold { get; set; } = 0.012f;
    /// <summary>Trims the stronger grip motor. XInput pads (8BitDo 2C included) use a heavier left motor.</summary>
    public float LeftMotorScale { get; set; } = 0.85f;
    public float RightMotorScale { get; set; } = 1.0f;

    public float SmoothingSeconds { get; set; } = 0.08f;
    public float AttackSeconds { get; set; } = 0.04f;
    public float ReleaseSeconds { get; set; } = 0.16f;
    public float FadeOutSeconds { get; set; } = 0.35f;
    public int TelemetryTimeoutMs { get; set; } = 400;

    public float EngineBase { get; set; } = 0.03f;
    public float EngineGain { get; set; } = 0.22f;
    public float EngineExponent { get; set; } = 1.6f;
    public float EngineModulation { get; set; } = 0.18f;
    public float EngineModulationHz { get; set; } = 12f;
    public float EngineRedlineModHz { get; set; } = 28f;
    public float EngineThrottleBlend { get; set; } = 0.72f;
    public float BoostGain { get; set; } = 0.18f;

    public float RoadGain { get; set; } = 0.14f;
    public float RoadSurfaceGain { get; set; } = 0.55f;
    public float RoadRumbleStripGain { get; set; } = 0.35f;
    public float RoadPuddleGain { get; set; } = 0.40f;
    public float MaxSpeedKmh { get; set; } = 300f;

    public float HandbrakeGain { get; set; } = 0.38f;

    public float SlipGain { get; set; } = 0.28f;
    public float WheelspinGain { get; set; } = 0.18f;

    public float DriftGain { get; set; } = 0.32f;
    public float DriftThreshold { get; set; } = 0.22f;

    public float AbsGain { get; set; } = 0.42f;
    public float AbsFrequencyHz { get; set; } = 14f;
    public float AbsPulseWidth { get; set; } = 0.42f;

    public float TractionGain { get; set; } = 0.30f;
    public float TractionFrequencyHz { get; set; } = 9f;

    public float GearShiftGain { get; set; } = 0.62f;
    public float GearShiftDurationMs { get; set; } = 160f;
    public float GearShiftDownshiftScale { get; set; } = 1.15f;

    public float ImpactGain { get; set; } = 0.85f;
    public float ImpactAttackMs { get; set; } = 80f;
    public float ImpactDecayMs { get; set; } = 220f;
    public float ImpactPriority { get; set; } = 0.85f;

    public bool EngineEnabled { get; set; } = true;
    public bool BoostEnabled { get; set; } = true;
    public bool RoadEnabled { get; set; } = true;
    public bool SlipEnabled { get; set; } = true;
    public bool DriftEnabled { get; set; } = true;
    public bool AbsEnabled { get; set; } = true;
    public bool TractionEnabled { get; set; } = true;
    public bool HandbrakeEnabled { get; set; } = true;
    public bool GearEnabled { get; set; } = true;
    public bool ImpactEnabled { get; set; } = true;
    public bool GForceEnabled { get; set; } = true;
    public float GForceGain { get; set; } = 0.10f;

    public ResponseCurve EngineCurve { get; set; } = ResponseCurve.Power(1.6f);
    public ResponseCurve RoadCurve { get; set; } = ResponseCurve.Power(0.75f);
    public ResponseCurve SlipCurve { get; set; } = ResponseCurve.Linear();
    public ResponseCurve DriftCurve { get; set; } = ResponseCurve.Delayed(0.22f);
    public ResponseCurve AbsCurve { get; set; } = ResponseCurve.Linear();
    public ResponseCurve TractionCurve { get; set; } = ResponseCurve.Linear();
    public ResponseCurve GearCurve { get; set; } = ResponseCurve.Linear();
    public ResponseCurve ImpactCurve { get; set; } = ResponseCurve.Linear();
    public ResponseCurve GForceCurve { get; set; } = ResponseCurve.Linear();

    public ResponseCurve CurveFor(string aspect) => aspect switch
    {
        "Road" => RoadCurve,
        "Slip" => SlipCurve,
        "Drift" => DriftCurve,
        "ABS" => AbsCurve,
        "Traction" => TractionCurve,
        "Gear" => GearCurve,
        "Impact" => ImpactCurve,
        "G-force" => GForceCurve,
        "Handbrake" => DriftCurve,
        _ => EngineCurve
    };

    public bool IsEnabled(string aspect) => aspect switch
    {
        "Road" => RoadEnabled,
        "Slip" => SlipEnabled,
        "Drift" => DriftEnabled,
        "ABS" => AbsEnabled,
        "Traction" => TractionEnabled,
        "Gear" => GearEnabled,
        "Impact" => ImpactEnabled,
        "G-force" => GForceEnabled,
        "Boost" => BoostEnabled,
        "Handbrake" => HandbrakeEnabled,
        _ => EngineEnabled
    };

    public void SetEnabled(string aspect, bool enabled)
    {
        switch (aspect)
        {
            case "Road": RoadEnabled = enabled; break;
            case "Slip": SlipEnabled = enabled; break;
            case "Drift": DriftEnabled = enabled; break;
            case "ABS": AbsEnabled = enabled; break;
            case "Traction": TractionEnabled = enabled; break;
            case "Gear": GearEnabled = enabled; break;
            case "Impact": ImpactEnabled = enabled; break;
            case "G-force": GForceEnabled = enabled; break;
            case "Boost": BoostEnabled = enabled; break;
            case "Handbrake": HandbrakeEnabled = enabled; break;
            default: EngineEnabled = enabled; break;
        }
    }

    public void SetCurve(string aspect, ResponseCurve curve)
    {
        switch (aspect)
        {
            case "Road": RoadCurve = curve; break;
            case "Slip": SlipCurve = curve; break;
            case "Drift": DriftCurve = curve; break;
            case "ABS": AbsCurve = curve; break;
            case "Traction": TractionCurve = curve; break;
            case "Gear": GearCurve = curve; break;
            case "Impact": ImpactCurve = curve; break;
            case "G-force": GForceCurve = curve; break;
            case "Handbrake": DriftCurve = curve; break;
            default: EngineCurve = curve; break;
        }
    }

    public HapticSettings Clone()
    {
        var copy = (HapticSettings)MemberwiseClone();
        copy.EngineCurve = EngineCurve.Clone();
        copy.RoadCurve = RoadCurve.Clone();
        copy.SlipCurve = SlipCurve.Clone();
        copy.DriftCurve = DriftCurve.Clone();
        copy.AbsCurve = AbsCurve.Clone();
        copy.TractionCurve = TractionCurve.Clone();
        copy.GearCurve = GearCurve.Clone();
        copy.ImpactCurve = ImpactCurve.Clone();
        copy.GForceCurve = GForceCurve.Clone();
        return copy;
    }
}
