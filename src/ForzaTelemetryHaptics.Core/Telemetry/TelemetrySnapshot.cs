namespace ForzaTelemetryHaptics.Telemetry;

public enum TelemetryLinkState
{
    Waiting,
    Live,
    Stale,
    Stopped
}

public sealed class TelemetrySnapshot
{
    public TelemetryLinkState State { get; init; } = TelemetryLinkState.Waiting;
    public VehicleTelemetry Telemetry { get; init; } = VehicleTelemetry.Empty;
    public DateTime? LastPacketUtc { get; init; }
    public int PacketRateHz { get; init; }
    public long AcceptedPackets { get; init; }
    public long MalformedPackets { get; init; }
    public string LastError { get; init; } = string.Empty;
    public string BindEndpoint { get; init; } = string.Empty;
}
