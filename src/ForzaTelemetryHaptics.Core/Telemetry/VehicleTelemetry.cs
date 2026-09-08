namespace ForzaTelemetryHaptics.Telemetry;

/// <summary>
/// Normalized vehicle state consumed by the haptic engine.
/// Every value is tagged as Available, Derived, or Unavailable.
/// </summary>
public sealed class VehicleTelemetry
{
    public static VehicleTelemetry Empty { get; } = new();

    public bool IsDriving { get; init; }
    public uint TimestampMs { get; init; }
    public DateTime ReceivedUtc { get; init; }

    public float Rpm { get; init; }
    public float IdleRpm { get; init; }
    public float MaxRpm { get; init; }
    public float NormalizedRpm { get; init; }
    public bool EngineRunning { get; init; }

    public float SpeedMps { get; init; }
    public float SpeedKmh { get; init; }
    public float NormalizedSpeed { get; init; }

    public float Throttle { get; init; }
    public float Brake { get; init; }
    public float Clutch { get; init; }
    public float Handbrake { get; init; }
    public float Steer { get; init; }
    public int Gear { get; init; }

    public WheelCorner<float> SlipRatio { get; init; }
    public WheelCorner<float> SlipAngle { get; init; }
    public WheelCorner<float> CombinedSlip { get; init; }
    public WheelCorner<float> SurfaceRumble { get; init; }
    public WheelCorner<float> WheelRotationSpeed { get; init; }
    public WheelCorner<float> NormalizedSuspension { get; init; }
    public WheelCorner<float> SuspensionTravelMeters { get; init; }
    public WheelCorner<float> PuddleDepth { get; init; }
    public WheelCorner<bool> OnRumbleStrip { get; init; }

    public float Boost { get; init; }
    public float NormalizedBoost { get; init; }
    public bool BoostActive { get; init; }
    public float PuddleLeft { get; init; }
    public float PuddleRight { get; init; }
    public float SuspensionThudLeft { get; init; }
    public float SuspensionThudRight { get; init; }
    public float SuspensionThud => MathF.Max(SuspensionThudLeft, SuspensionThudRight);

    public float AggregateSlip { get; init; }
    public float FrontSlip { get; init; }
    public float RearSlip { get; init; }
    public float FrontLeftSlip { get; init; }
    public float FrontRightSlip { get; init; }
    public float RearLeftSlip { get; init; }
    public float RearRightSlip { get; init; }

    /// <summary>Local-space X: right. Available.</summary>
    public float AccelLateral { get; init; }

    /// <summary>Local-space Y: up. Available.</summary>
    public float AccelVertical { get; init; }

    /// <summary>Local-space Z: forward. Available.</summary>
    public float AccelLongitudinal { get; init; }

    public float SmashableVelDiff { get; init; }
    public float SmashableMass { get; init; }

    public int DrivetrainType { get; init; }
    public string DrivetrainLabel { get; init; } = "Unknown";

    public bool AbsActive { get; init; }
    public float AbsStrength { get; init; }
    public FieldSource AbsSource { get; init; } = FieldSource.Unavailable;

    public bool TractionActive { get; init; }
    public float TractionStrength { get; init; }
    public FieldSource TractionSource { get; init; } = FieldSource.Unavailable;

    public float SmashableImpact { get; init; }
    public float LandingImpact { get; init; }
    public float SpeedImpact { get; init; }
    public float AccelImpact { get; init; }
    public float ImpactMagnitude { get; init; }
    public bool GearShiftHint { get; init; }

    public string SurfaceSummary { get; init; } = "Unknown";

    public string GearLabel => Gear switch
    {
        0 => "R",
        >= 11 => "N",
        _ => Gear.ToString()
    };
}
