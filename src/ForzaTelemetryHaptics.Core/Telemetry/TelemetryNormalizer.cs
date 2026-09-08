namespace ForzaTelemetryHaptics.Telemetry;

/// <summary>
/// Converts a raw FH6 packet into the internal vehicle model.
/// ABS and traction-control flags are not on the wire; they are derived only
/// from slip + pedal + drivetrain, and tagged as Derived.
/// </summary>
public sealed class TelemetryNormalizer
{
    private readonly float _maxSpeedMps;
    private float _previousVerticalAccel;
    private float _previousAccelMagnitude;
    private float _previousSpeed;
    private float _previousRpm;
    private int? _previousGear;
    private DateTime _previousUtc;
    private bool _hasPreviousAccel;
    private bool _hasPreviousSuspension;
    private WheelCorner<float> _previousSuspension;

    public TelemetryNormalizer(float maxSpeedMps = 83.33f)
    {
        _maxSpeedMps = Math.Max(maxSpeedMps, 1f);
    }

    public VehicleTelemetry Normalize(RawFh6Packet raw, DateTime receivedUtc)
    {
        var rpm = Sanitize(raw.CurrentEngineRpm);
        var idle = Math.Max(Sanitize(raw.EngineIdleRpm), 0f);
        var maxRpm = Math.Max(Sanitize(raw.EngineMaxRpm), idle + 1f);
        var speed = Math.Max(Sanitize(raw.Speed), 0f);
        var throttle = MathUtil.Clamp01(raw.Accel / 255f);
        var brake = MathUtil.Clamp01(raw.Brake / 255f);
        var clutch = MathUtil.Clamp01(raw.Clutch / 255f);
        var handbrake = MathUtil.Clamp01(raw.HandBrake / 255f);
        var steer = Math.Clamp(raw.Steer / 127f, -1f, 1f);

        var slipRatio = new WheelCorner<float>(
            Sanitize(raw.TireSlipRatioFrontLeft),
            Sanitize(raw.TireSlipRatioFrontRight),
            Sanitize(raw.TireSlipRatioRearLeft),
            Sanitize(raw.TireSlipRatioRearRight));

        var slipAngle = new WheelCorner<float>(
            Sanitize(raw.TireSlipAngleFrontLeft),
            Sanitize(raw.TireSlipAngleFrontRight),
            Sanitize(raw.TireSlipAngleRearLeft),
            Sanitize(raw.TireSlipAngleRearRight));

        var combined = new WheelCorner<float>(
            Sanitize(raw.TireCombinedSlipFrontLeft),
            Sanitize(raw.TireCombinedSlipFrontRight),
            Sanitize(raw.TireCombinedSlipRearLeft),
            Sanitize(raw.TireCombinedSlipRearRight));

        var flSlip = MathUtil.Clamp01(MathF.Abs(combined.FrontLeft));
        var frSlip = MathUtil.Clamp01(MathF.Abs(combined.FrontRight));
        var rlSlip = MathUtil.Clamp01(MathF.Abs(combined.RearLeft));
        var rrSlip = MathUtil.Clamp01(MathF.Abs(combined.RearRight));
        var frontSlip = MathF.Max(flSlip, frSlip);
        var rearSlip = MathF.Max(rlSlip, rrSlip);
        var aggregateSlip = MathUtil.Max4(flSlip, frSlip, rlSlip, rrSlip);

        var surface = new WheelCorner<float>(
            Math.Max(Sanitize(raw.SurfaceRumbleFrontLeft), 0f),
            Math.Max(Sanitize(raw.SurfaceRumbleFrontRight), 0f),
            Math.Max(Sanitize(raw.SurfaceRumbleRearLeft), 0f),
            Math.Max(Sanitize(raw.SurfaceRumbleRearRight), 0f));

        var rumbleStrip = new WheelCorner<bool>(
            raw.WheelOnRumbleStripFrontLeft != 0,
            raw.WheelOnRumbleStripFrontRight != 0,
            raw.WheelOnRumbleStripRearLeft != 0,
            raw.WheelOnRumbleStripRearRight != 0);

        var puddle = new WheelCorner<float>(
            MathUtil.Clamp01(Math.Max(Sanitize(raw.WheelInPuddleDepthFrontLeft), 0f)),
            MathUtil.Clamp01(Math.Max(Sanitize(raw.WheelInPuddleDepthFrontRight), 0f)),
            MathUtil.Clamp01(Math.Max(Sanitize(raw.WheelInPuddleDepthRearLeft), 0f)),
            MathUtil.Clamp01(Math.Max(Sanitize(raw.WheelInPuddleDepthRearRight), 0f)));
        var puddleLeft = MathF.Max(puddle.FrontLeft, puddle.RearLeft);
        var puddleRight = MathF.Max(puddle.FrontRight, puddle.RearRight);

        var suspensionMeters = new WheelCorner<float>(
            Sanitize(raw.SuspensionTravelMetersFrontLeft),
            Sanitize(raw.SuspensionTravelMetersFrontRight),
            Sanitize(raw.SuspensionTravelMetersRearLeft),
            Sanitize(raw.SuspensionTravelMetersRearRight));

        var boost = Math.Max(Sanitize(raw.Boost), 0f);
        var normalizedBoost = boost > 1.5f ? MathUtil.Clamp01(boost / 18f) : MathUtil.Clamp01(boost);

        var accelLat = Sanitize(raw.AccelerationX);
        var accelVert = Sanitize(raw.AccelerationY);
        var accelLong = Sanitize(raw.AccelerationZ);
        var accelMag = MathF.Sqrt(accelLat * accelLat + accelVert * accelVert + accelLong * accelLong);

        var smashVel = Math.Abs(Sanitize(raw.SmashableVelDiff));
        var smashMass = Math.Max(Sanitize(raw.SmashableMass), 0f);
        var smashableImpact = smashVel > 0.12f || smashMass > 4f
            ? MathUtil.Clamp01(smashVel / 4f + smashMass / 250f)
            : 0f;

        var thudLeft = 0f;
        var thudRight = 0f;
        if (_hasPreviousSuspension)
        {
            thudLeft = CornerThud(_previousSuspension.FrontLeft, suspensionMeters.FrontLeft)
                       + CornerThud(_previousSuspension.RearLeft, suspensionMeters.RearLeft);
            thudRight = CornerThud(_previousSuspension.FrontRight, suspensionMeters.FrontRight)
                        + CornerThud(_previousSuspension.RearRight, suspensionMeters.RearRight);
            thudLeft = MathUtil.Clamp01(thudLeft);
            thudRight = MathUtil.Clamp01(thudRight);
        }

        _previousSuspension = suspensionMeters;
        _hasPreviousSuspension = true;

        var landingImpact = 0f;
        var accelImpact = 0f;
        var speedImpact = 0f;
        var gearShiftHint = false;
        var dt = _hasPreviousAccel
            ? Math.Clamp((float)(receivedUtc - _previousUtc).TotalSeconds, 0.008f, 0.25f)
            : 0.016f;

        if (_hasPreviousAccel)
        {
            var verticalDelta = MathF.Abs(accelVert - _previousVerticalAccel);
            if (verticalDelta > 7f && speed > 1.5f)
            {
                landingImpact = MathUtil.Clamp01((verticalDelta - 7f) / 22f);
            }

            var magDelta = MathF.Abs(accelMag - _previousAccelMagnitude);
            if (magDelta > 8f)
            {
                accelImpact = MathUtil.Clamp01((magDelta - 8f) / 18f);
            }

            var speedDrop = _previousSpeed - speed;
            var decel = speedDrop / dt;
            var brakingHard = brake > 0.7f;
            if (speedDrop > 2.0f && decel > 22f && !brakingHard)
            {
                speedImpact = MathUtil.Clamp01(speedDrop / 12f);
            }
            else if (speedDrop > 4.5f && decel > 30f)
            {
                speedImpact = MathUtil.Clamp01(speedDrop / 10f);
            }

            var rpmDrop = _previousRpm - rpm;
            if (rpmDrop > Math.Max(350f, _previousRpm * 0.10f)
                && throttle > 0.12f
                && brake < 0.25f
                && speed > 2f
                && _previousGear is int lastGear
                && lastGear == raw.Gear)
            {
                gearShiftHint = true;
            }
        }

        _previousVerticalAccel = accelVert;
        _previousAccelMagnitude = accelMag;
        _previousSpeed = speed;
        _previousRpm = rpm;
        _previousGear = raw.Gear;
        _previousUtc = receivedUtc;
        _hasPreviousAccel = true;

        // Horizon often keeps sending packets in free roam with IsRaceOn = 0.
        var driving = raw.IsRaceOn != 0 || rpm > 200f || speed > 0.35f;
        var engineRunning = rpm > Math.Max(idle * 0.35f, 200f);
        var normalizedRpm = MathUtil.Clamp01(MathUtil.SaturateDivide(rpm - idle, maxRpm - idle));
        var normalizedSpeed = MathUtil.Clamp01(speed / _maxSpeedMps);

        DeriveAbs(brake, speed, slipRatio, out var absActive, out var absStrength);
        DeriveTraction(raw.DrivetrainType, throttle, speed, slipRatio, out var tcsActive, out var tcsStrength);

        return new VehicleTelemetry
        {
            IsDriving = driving,
            TimestampMs = raw.TimestampMs,
            ReceivedUtc = receivedUtc,
            Rpm = rpm,
            IdleRpm = idle,
            MaxRpm = maxRpm,
            NormalizedRpm = normalizedRpm,
            EngineRunning = engineRunning,
            SpeedMps = speed,
            SpeedKmh = speed * 3.6f,
            NormalizedSpeed = normalizedSpeed,
            Throttle = throttle,
            Brake = brake,
            Clutch = clutch,
            Handbrake = handbrake,
            Steer = steer,
            Gear = raw.Gear,
            SlipRatio = slipRatio,
            SlipAngle = slipAngle,
            CombinedSlip = combined,
            SurfaceRumble = surface,
            WheelRotationSpeed = new WheelCorner<float>(
                Sanitize(raw.WheelRotationSpeedFrontLeft),
                Sanitize(raw.WheelRotationSpeedFrontRight),
                Sanitize(raw.WheelRotationSpeedRearLeft),
                Sanitize(raw.WheelRotationSpeedRearRight)),
            NormalizedSuspension = new WheelCorner<float>(
                Sanitize(raw.NormalizedSuspensionTravelFrontLeft),
                Sanitize(raw.NormalizedSuspensionTravelFrontRight),
                Sanitize(raw.NormalizedSuspensionTravelRearLeft),
                Sanitize(raw.NormalizedSuspensionTravelRearRight)),
            SuspensionTravelMeters = suspensionMeters,
            PuddleDepth = puddle,
            OnRumbleStrip = rumbleStrip,
            Boost = boost,
            NormalizedBoost = normalizedBoost,
            BoostActive = normalizedBoost > 0.08f,
            PuddleLeft = puddleLeft,
            PuddleRight = puddleRight,
            SuspensionThudLeft = thudLeft,
            SuspensionThudRight = thudRight,
            AggregateSlip = aggregateSlip,
            FrontSlip = frontSlip,
            RearSlip = rearSlip,
            FrontLeftSlip = flSlip,
            FrontRightSlip = frSlip,
            RearLeftSlip = rlSlip,
            RearRightSlip = rrSlip,
            AccelLateral = accelLat,
            AccelVertical = accelVert,
            AccelLongitudinal = accelLong,
            SmashableVelDiff = smashVel,
            SmashableMass = smashMass,
            DrivetrainType = raw.DrivetrainType,
            DrivetrainLabel = raw.DrivetrainType switch
            {
                0 => "FWD",
                1 => "RWD",
                2 => "AWD",
                _ => "Unknown"
            },
            AbsActive = absActive,
            AbsStrength = absStrength,
            AbsSource = FieldSource.Derived,
            TractionActive = tcsActive,
            TractionStrength = tcsStrength,
            TractionSource = FieldSource.Derived,
            SmashableImpact = smashableImpact,
            LandingImpact = landingImpact,
            SpeedImpact = speedImpact,
            AccelImpact = accelImpact,
            ImpactMagnitude = MathUtil.Max4(
                smashableImpact,
                landingImpact,
                speedImpact,
                Math.Max(accelImpact, MathF.Max(thudLeft, thudRight))),
            GearShiftHint = gearShiftHint,
            SurfaceSummary = DescribeSurface(surface, rumbleStrip, puddleLeft, puddleRight)
        };
    }

    public void Reset()
    {
        _hasPreviousAccel = false;
        _hasPreviousSuspension = false;
        _previousSuspension = default;
        _previousVerticalAccel = 0f;
        _previousAccelMagnitude = 0f;
        _previousSpeed = 0f;
        _previousRpm = 0f;
        _previousGear = null;
    }

    public static void DeriveAbs(
        float brake,
        float speedMps,
        WheelCorner<float> slipRatio,
        out bool active,
        out float strength)
    {
        var lockSlip = MathUtil.AbsMax(
            Math.Min(slipRatio.FrontLeft, 0f),
            Math.Min(slipRatio.FrontRight, 0f),
            Math.Min(slipRatio.RearLeft, 0f),
            Math.Min(slipRatio.RearRight, 0f));

        active = brake > 0.55f && speedMps > 4f && lockSlip > 0.22f;
        strength = active ? MathUtil.Clamp01((lockSlip - 0.22f) / 0.7f) * MathUtil.Clamp01(brake) : 0f;
    }

    public static void DeriveTraction(
        int drivetrain,
        float throttle,
        float speedMps,
        WheelCorner<float> slipRatio,
        out bool active,
        out float strength)
    {
        var spin = DrivenWheelSpin(drivetrain, slipRatio);
        active = throttle > 0.28f && speedMps > 0.8f && spin > 0.32f;
        strength = active ? MathUtil.Clamp01((spin - 0.32f) / 0.8f) * MathUtil.Clamp01(throttle) : 0f;
    }

    public static float DrivenWheelSpin(int drivetrain, WheelCorner<float> slipRatio)
    {
        float Positive(float value) => Math.Max(value, 0f);

        return drivetrain switch
        {
            0 => Math.Max(Positive(slipRatio.FrontLeft), Positive(slipRatio.FrontRight)),
            1 => Math.Max(Positive(slipRatio.RearLeft), Positive(slipRatio.RearRight)),
            _ => MathUtil.Max4(
                Positive(slipRatio.FrontLeft),
                Positive(slipRatio.FrontRight),
                Positive(slipRatio.RearLeft),
                Positive(slipRatio.RearRight))
        };
    }

    private static string DescribeSurface(
        WheelCorner<float> surface,
        WheelCorner<bool> rumble,
        float puddleLeft,
        float puddleRight)
    {
        if (rumble.FrontLeft || rumble.FrontRight || rumble.RearLeft || rumble.RearRight)
        {
            return "Rumble strip";
        }

        if (puddleLeft > 0.08f || puddleRight > 0.08f)
        {
            return "Puddle";
        }

        var peak = MathUtil.Max4(surface.FrontLeft, surface.FrontRight, surface.RearLeft, surface.RearRight);
        if (peak < 0.05f) return "Smooth";
        if (peak < 0.25f) return "Light texture";
        if (peak < 0.55f) return "Rough";
        return "Very rough";
    }

    private static float CornerThud(float previousMeters, float currentMeters)
    {
        var compression = previousMeters - currentMeters;
        if (compression < 0.012f)
        {
            return 0f;
        }

        return MathUtil.Clamp01((compression - 0.012f) / 0.035f);
    }

    private static float Sanitize(float value) => float.IsFinite(value) ? value : 0f;
}
