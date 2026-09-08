using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Tests;

public class TelemetryParserTests
{
    private readonly Fh6TelemetryParser _parser = new();

    [Fact]
    public void Layout_IsExactly324Bytes()
    {
        Assert.Equal(324, Fh6PacketLayout.PacketSize);
        Assert.Equal(323, Fh6PacketLayout.TrailingByte);
        Assert.Equal(232, Fh6PacketLayout.CarGroup);
        Assert.Equal(236, Fh6PacketLayout.SmashableVelDiff);
        Assert.Equal(240, Fh6PacketLayout.SmashableMass);
        Assert.Equal(244, Fh6PacketLayout.PositionX);
        Assert.Equal(319, Fh6PacketLayout.Gear);
    }

    [Fact]
    public void RoundTrip_PreservesDocumentedFields()
    {
        var raw = SamplePacket();
        var bytes = Fh6PacketWriter.Write(raw);
        Assert.Equal(324, bytes.Length);
        Assert.True(_parser.TryParse(bytes, out var parsed, out var error));
        Assert.Null(error);
        Assert.NotNull(parsed);
        Assert.Equal(1, parsed!.IsRaceOn);
        Assert.Equal(1234u, parsed.TimestampMs);
        Assert.Equal(7500f, parsed.EngineMaxRpm);
        Assert.Equal(800f, parsed.EngineIdleRpm);
        Assert.Equal(4200f, parsed.CurrentEngineRpm);
        Assert.Equal(33.5f, parsed.Speed, 3);
        Assert.Equal(200, parsed.Accel);
        Assert.Equal(4, parsed.Gear);
        Assert.Equal(-40, parsed.Steer);
        Assert.Equal(7.5f, parsed.SmashableVelDiff, 3);
        Assert.Equal(180f, parsed.SmashableMass, 3);
        Assert.Equal(1, parsed.WheelOnRumbleStripFrontLeft);
    }

    [Fact]
    public void Malformed_WrongLength_IsRejected()
    {
        Assert.False(_parser.TryParse(new byte[100], out var packet, out var error));
        Assert.Null(packet);
        Assert.Contains("100", error);
    }

    [Fact]
    public void Malformed_Empty_IsRejected()
    {
        Assert.False(_parser.TryParse(ReadOnlySpan<byte>.Empty, out _, out var error));
        Assert.Contains("0", error);
    }

    [Fact]
    public void Malformed_NonFiniteRpm_IsRejected()
    {
        var raw = SamplePacket();
        raw = new RawFh6Packet
        {
            IsRaceOn = raw.IsRaceOn,
            TimestampMs = raw.TimestampMs,
            EngineMaxRpm = raw.EngineMaxRpm,
            EngineIdleRpm = raw.EngineIdleRpm,
            CurrentEngineRpm = float.NaN,
            Speed = raw.Speed
        };
        var bytes = Fh6PacketWriter.Write(raw);
        Assert.False(_parser.TryParse(bytes, out var packet, out var error));
        Assert.Null(packet);
        Assert.Contains("non-finite", error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parser_DoesNotThrow_OnGarbage()
    {
        var garbage = new byte[324];
        Random.Shared.NextBytes(garbage);
        var ok = _parser.TryParse(garbage, out _, out _);
        Assert.True(ok || !ok);
    }

    internal static RawFh6Packet SamplePacket() => new()
    {
        IsRaceOn = 1,
        TimestampMs = 1234,
        EngineMaxRpm = 7500,
        EngineIdleRpm = 800,
        CurrentEngineRpm = 4200,
        AccelerationX = 1.2f,
        AccelerationY = 9.8f,
        AccelerationZ = 3.4f,
        Speed = 33.5f,
        Accel = 200,
        Brake = 10,
        Gear = 4,
        Steer = -40,
        SmashableVelDiff = 7.5f,
        SmashableMass = 180f,
        WheelOnRumbleStripFrontLeft = 1,
        TireCombinedSlipFrontLeft = 0.2f,
        TireCombinedSlipFrontRight = 0.21f,
        TireCombinedSlipRearLeft = 0.3f,
        TireCombinedSlipRearRight = 0.28f,
        TireSlipRatioRearLeft = 0.4f,
        TireSlipRatioRearRight = 0.35f,
        SurfaceRumbleRearLeft = 0.15f,
        DrivetrainType = 1
    };
}
