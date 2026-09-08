namespace ForzaTelemetryHaptics.Configuration;

public sealed class AppConfig
{
    public string TelemetryBindAddress { get; set; } = "127.0.0.1";
    public int TelemetryPort { get; set; } = 5000;
    public string ActiveProfile { get; set; } = "Default";
    public int ControllerIndex { get; set; } = -1;
    public int HapticRateHz { get; set; } = 100;
    public HapticSettings Haptics { get; set; } = new();
}
