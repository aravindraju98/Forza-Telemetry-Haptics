namespace ForzaTelemetryHaptics.Telemetry;

public interface ITelemetryParser
{
    bool TryParse(ReadOnlySpan<byte> data, out RawFh6Packet? packet, out string? error);
}
