using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Tests;

public class NormalizationTests
{
    [Fact]
    public void Rpm_IsNormalizedBetweenIdleAndRedline()
    {
        var normalizer = new TelemetryNormalizer();
        var raw = TelemetryParserTests.SamplePacket();
        raw = WithRpm(raw, 800, 800, 7500);
        var idle = normalizer.Normalize(raw, DateTime.UtcNow);
        Assert.Equal(0f, idle.NormalizedRpm, 3);

        raw = WithRpm(raw, 7500, 800, 7500);
        var redline = normalizer.Normalize(raw, DateTime.UtcNow);
        Assert.Equal(1f, redline.NormalizedRpm, 3);

        raw = WithRpm(raw, 4150, 800, 7500);
        var mid = normalizer.Normalize(raw, DateTime.UtcNow);
        Assert.InRange(mid.NormalizedRpm, 0.49f, 0.51f);
    }

    [Fact]
    public void Rpm_ZeroMax_DoesNotDivideByZero()
    {
        var raw = WithRpm(TelemetryParserTests.SamplePacket(), 1000, 0, 0);
        var tel = new TelemetryNormalizer().Normalize(raw, DateTime.UtcNow);
        Assert.InRange(tel.NormalizedRpm, 0f, 1f);
        Assert.True(float.IsFinite(tel.NormalizedRpm));
    }

    [Fact]
    public void Slip_IsClampedToUnitRange()
    {
        var raw = TelemetryParserTests.SamplePacket();
        raw = new RawFh6Packet
        {
            IsRaceOn = 1,
            EngineMaxRpm = 7000,
            EngineIdleRpm = 800,
            CurrentEngineRpm = 2000,
            TireCombinedSlipFrontLeft = 4f,
            TireCombinedSlipFrontRight = -2f,
            TireCombinedSlipRearLeft = 0.4f,
            TireCombinedSlipRearRight = 0.1f
        };
        var tel = new TelemetryNormalizer().Normalize(raw, DateTime.UtcNow);
        Assert.Equal(1f, tel.FrontLeftSlip);
        Assert.Equal(1f, tel.FrontRightSlip);
        Assert.InRange(tel.AggregateSlip, 0f, 1f);
    }

    [Fact]
    public void Inputs_AreNormalizedFromBytes()
    {
        var raw = TelemetryParserTests.SamplePacket();
        var tel = new TelemetryNormalizer().Normalize(raw, DateTime.UtcNow);
        Assert.Equal(200f / 255f, tel.Throttle, 3);
        Assert.InRange(tel.Steer, -1f, 1f);
        Assert.Equal(FieldSource.Derived, tel.AbsSource);
        Assert.Equal(FieldSource.Derived, tel.TractionSource);
    }

    [Fact]
    public void Abs_IsDerivedFromHardBrakeAndLockingWheels()
    {
        TelemetryNormalizer.DeriveAbs(0.9f, 20f, new WheelCorner<float>(-0.8f, -0.7f, -0.2f, -0.2f), out var active, out var strength);
        Assert.True(active);
        Assert.True(strength > 0.2f);

        TelemetryNormalizer.DeriveAbs(0.1f, 20f, new WheelCorner<float>(-0.8f, -0.7f, 0, 0), out var off, out var zero);
        Assert.False(off);
        Assert.Equal(0f, zero);
    }

    [Fact]
    public void SpeedDrop_ProducesDerivedImpact()
    {
        var normalizer = new TelemetryNormalizer();
        var moving = WithSpeed(TelemetryParserTests.SamplePacket(), 30f, brake: 0);
        normalizer.Normalize(moving, DateTime.UtcNow);
        var crashed = WithSpeed(moving, 18f, brake: 0);
        var tel = normalizer.Normalize(crashed, DateTime.UtcNow.AddMilliseconds(16));
        Assert.True(tel.SpeedImpact > 0.15f);
        Assert.True(tel.ImpactMagnitude > 0.15f);
    }

    [Fact]
    public void SmashableHit_ProducesImpact()
    {
        var raw = TelemetryParserTests.SamplePacket();
        raw = new RawFh6Packet
        {
            IsRaceOn = 1,
            EngineMaxRpm = 7000,
            EngineIdleRpm = 800,
            CurrentEngineRpm = 2500,
            Speed = 20,
            SmashableVelDiff = 3.2f,
            SmashableMass = 40f
        };
        var tel = new TelemetryNormalizer().Normalize(raw, DateTime.UtcNow);
        Assert.True(tel.SmashableImpact > 0.1f);
        Assert.True(tel.ImpactMagnitude > 0.1f);
    }

    [Fact]
    public void FreeRoam_StillCountsAsDrivingWhenRaceFlagIsOff()
    {
        var raw = WithRpm(TelemetryParserTests.SamplePacket(), 2800, 800, 7500);
        raw = new RawFh6Packet
        {
            IsRaceOn = 0,
            EngineMaxRpm = 7500,
            EngineIdleRpm = 800,
            CurrentEngineRpm = 2800,
            Speed = 18
        };
        var tel = new TelemetryNormalizer().Normalize(raw, DateTime.UtcNow);
        Assert.True(tel.IsDriving);
        Assert.True(tel.EngineRunning);
    }

    [Fact]
    public void BoostAndPuddle_AreCopiedFromThePacket()
    {
        var raw = TelemetryParserTests.SamplePacket();
        raw = new RawFh6Packet
        {
            IsRaceOn = 1,
            EngineMaxRpm = 7000,
            EngineIdleRpm = 800,
            CurrentEngineRpm = 3000,
            Speed = 20,
            Boost = 9f,
            WheelInPuddleDepthFrontLeft = 0.6f,
            WheelInPuddleDepthRearLeft = 0.4f,
            WheelInPuddleDepthFrontRight = 0.05f
        };
        var tel = new TelemetryNormalizer().Normalize(raw, DateTime.UtcNow);
        Assert.True(tel.BoostActive);
        Assert.InRange(tel.NormalizedBoost, 0.4f, 0.6f);
        Assert.InRange(tel.PuddleLeft, 0.59f, 0.61f);
        Assert.InRange(tel.PuddleRight, 0.04f, 0.06f);
    }

    [Fact]
    public void SuspensionCompression_ProducesDirectionalThud()
    {
        var normalizer = new TelemetryNormalizer();
        var first = TelemetryParserTests.SamplePacket();
        first = new RawFh6Packet
        {
            IsRaceOn = 1,
            EngineMaxRpm = 7000,
            EngineIdleRpm = 800,
            CurrentEngineRpm = 2500,
            Speed = 18,
            SuspensionTravelMetersFrontLeft = 0.08f,
            SuspensionTravelMetersRearLeft = 0.08f,
            SuspensionTravelMetersFrontRight = 0.08f,
            SuspensionTravelMetersRearRight = 0.08f
        };
        normalizer.Normalize(first, DateTime.UtcNow);
        var compressed = new RawFh6Packet
        {
            IsRaceOn = 1,
            EngineMaxRpm = 7000,
            EngineIdleRpm = 800,
            CurrentEngineRpm = 2500,
            Speed = 18,
            SuspensionTravelMetersFrontLeft = 0.02f,
            SuspensionTravelMetersRearLeft = 0.03f,
            SuspensionTravelMetersFrontRight = 0.075f,
            SuspensionTravelMetersRearRight = 0.075f
        };
        var tel = normalizer.Normalize(compressed, DateTime.UtcNow.AddMilliseconds(16));
        Assert.True(tel.SuspensionThudLeft > 0.2f);
        Assert.True(tel.SuspensionThudLeft > tel.SuspensionThudRight);
        Assert.True(tel.ImpactMagnitude > 0.2f);
    }

    [Fact]
    public void Traction_UsesDrivenWheels()
    {
        var spin = new WheelCorner<float>(0.1f, 0.1f, 0.9f, 0.85f);
        TelemetryNormalizer.DeriveTraction(1, 0.8f, 8f, spin, out var rwd, out _);
        Assert.True(rwd);
        TelemetryNormalizer.DeriveTraction(0, 0.8f, 8f, spin, out var fwd, out _);
        Assert.False(fwd);
    }

    private static RawFh6Packet WithRpm(RawFh6Packet raw, float rpm, float idle, float max) => new()
    {
        IsRaceOn = raw.IsRaceOn,
        TimestampMs = raw.TimestampMs,
        CurrentEngineRpm = rpm,
        EngineIdleRpm = idle,
        EngineMaxRpm = max,
        Speed = raw.Speed,
        Accel = raw.Accel,
        Brake = raw.Brake,
        Gear = raw.Gear,
        Steer = raw.Steer
    };

    private static RawFh6Packet WithSpeed(RawFh6Packet raw, float speed, byte brake) => new()
    {
        IsRaceOn = raw.IsRaceOn == 0 ? 1 : raw.IsRaceOn,
        TimestampMs = raw.TimestampMs,
        CurrentEngineRpm = raw.CurrentEngineRpm == 0 ? 3000 : raw.CurrentEngineRpm,
        EngineIdleRpm = raw.EngineIdleRpm == 0 ? 800 : raw.EngineIdleRpm,
        EngineMaxRpm = raw.EngineMaxRpm == 0 ? 7500 : raw.EngineMaxRpm,
        Speed = speed,
        Brake = brake,
        Accel = raw.Accel
    };
}
