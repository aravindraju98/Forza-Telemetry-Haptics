using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics;

public interface IHapticEffect
{
    string Name { get; }
    HapticSignal Update(VehicleTelemetry telemetry, HapticSettings settings, float deltaSeconds);
    void Reset();
}
