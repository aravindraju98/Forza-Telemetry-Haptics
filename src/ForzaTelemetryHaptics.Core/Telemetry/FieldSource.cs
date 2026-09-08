namespace ForzaTelemetryHaptics.Telemetry;

/// <summary>
/// How a value entered the internal vehicle model.
/// Available = present on the FH6 Data Out wire.
/// Derived = computed from available fields.
/// Unavailable = not present in FH6 Data Out; never fabricated.
/// </summary>
public enum FieldSource
{
    Available,
    Derived,
    Unavailable
}
