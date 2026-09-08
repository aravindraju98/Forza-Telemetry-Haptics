using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Tests;

public class TelemetryReceiverTests
{
    [Fact]
    public void Snapshot_BeforePackets_IsWaiting()
    {
        using var receiver = new TelemetryReceiver(new Fh6TelemetryParser(), new TelemetryNormalizer());
        var snap = receiver.Snapshot(TimeSpan.FromMilliseconds(400));
        Assert.Equal(TelemetryLinkState.Waiting, snap.State);
        Assert.Equal(0, snap.PacketRateHz);
        Assert.Null(snap.LastPacketUtc);
    }

    [Fact]
    public void EngineTimeout_AfterLiveSignal_ReachesZero()
    {
        var engine = new ForzaTelemetryHaptics.Haptics.HapticEngine();
        var settings = new ForzaTelemetryHaptics.Configuration.HapticSettings
        {
            FadeOutSeconds = 0.04f,
            MaximumRumble = 1f,
            EngineBase = 0.2f
        };
        var live = new VehicleTelemetry
        {
            IsDriving = true,
            EngineRunning = true,
            Rpm = 5000,
            IdleRpm = 800,
            MaxRpm = 7500,
            NormalizedRpm = 0.6f
        };
        var active = engine.Update(live, settings, 0.02f, true);
        Assert.True(active.Peak > 0f);

        var faded = active;
        for (var i = 0; i < 50; i++)
        {
            faded = engine.Update(live, settings, 0.02f, false);
        }

        Assert.Equal(0f, faded.LeftMotor);
        Assert.Equal(0f, faded.RightMotor);
    }
}
