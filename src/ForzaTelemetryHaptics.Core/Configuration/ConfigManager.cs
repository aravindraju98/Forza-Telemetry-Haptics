using System.Text.Json;
using System.Text.Json.Serialization;
using ForzaTelemetryHaptics.Haptics;

namespace ForzaTelemetryHaptics.Configuration;

public sealed class ConfigManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never
    };

    private readonly string _path;
    public AppConfig Current { get; private set; } = new();

    public ConfigManager(string path)
    {
        _path = path;
    }

    public AppConfig LoadOrCreate()
    {
        try
        {
            if (File.Exists(_path))
            {
                var json = File.ReadAllText(_path);
                Current = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
            }
            else
            {
                Current = new AppConfig();
                Save();
            }
        }
        catch
        {
            Current = new AppConfig();
        }

        Sanitize(Current);
        return Current;
    }

    public void Save()
    {
        Sanitize(Current);
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_path, JsonSerializer.Serialize(Current, JsonOptions));
    }

    public void ReplaceHaptics(HapticSettings settings)
    {
        Current.Haptics = settings.Clone();
    }

    internal static void Sanitize(AppConfig config)
    {
        config.TelemetryPort = Math.Clamp(config.TelemetryPort, 1, 65535);
        config.HapticRateHz = Math.Clamp(config.HapticRateHz, 20, 250);
        config.Haptics ??= new HapticSettings();
        var defaults = new HapticSettings();
        config.Haptics.EngineCurve = ResponseCurve.Ensure(config.Haptics.EngineCurve, defaults.EngineCurve);
        config.Haptics.BoostCurve = ResponseCurve.Ensure(config.Haptics.BoostCurve, defaults.BoostCurve);
        config.Haptics.RoadCurve = ResponseCurve.Ensure(config.Haptics.RoadCurve, defaults.RoadCurve);
        config.Haptics.SlipCurve = ResponseCurve.Ensure(config.Haptics.SlipCurve, defaults.SlipCurve);
        config.Haptics.DriftCurve = ResponseCurve.Ensure(config.Haptics.DriftCurve, defaults.DriftCurve);
        config.Haptics.AbsCurve = ResponseCurve.Ensure(config.Haptics.AbsCurve, defaults.AbsCurve);
        config.Haptics.TractionCurve = ResponseCurve.Ensure(config.Haptics.TractionCurve, defaults.TractionCurve);
        config.Haptics.GearCurve = ResponseCurve.Ensure(config.Haptics.GearCurve, defaults.GearCurve);
        config.Haptics.ImpactCurve = ResponseCurve.Ensure(config.Haptics.ImpactCurve, defaults.ImpactCurve);
        config.Haptics.GForceCurve = ResponseCurve.Ensure(config.Haptics.GForceCurve, defaults.GForceCurve);
        config.Haptics.MaximumRumble = Math.Clamp(config.Haptics.MaximumRumble, 0.01f, 1f);
        config.Haptics.GlobalGain = Math.Clamp(config.Haptics.GlobalGain, 0f, 2f);
        config.Haptics.TelemetryTimeoutMs = Math.Clamp(config.Haptics.TelemetryTimeoutMs, 50, 5000);
        if (string.IsNullOrWhiteSpace(config.TelemetryBindAddress))
        {
            config.TelemetryBindAddress = "127.0.0.1";
        }
    }
}
