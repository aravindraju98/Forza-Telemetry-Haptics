using System.Text.Json;

namespace ForzaTelemetryHaptics.Configuration;

public sealed class ProfileManager
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _directory;

    public ProfileManager(string directory)
    {
        _directory = directory;
        Directory.CreateDirectory(_directory);
        EnsureBuiltInProfiles();
    }

    public IReadOnlyList<string> ListProfiles()
    {
        return Directory.GetFiles(_directory, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public bool TryLoad(string name, out HapticSettings settings, out string error)
    {
        settings = new HapticSettings();
        error = string.Empty;
        var path = PathFor(name);
        if (!File.Exists(path))
        {
            error = $"Profile '{name}' was not found.";
            return false;
        }

        try
        {
            settings = JsonSerializer.Deserialize<HapticSettings>(File.ReadAllText(path), JsonOptions) ?? new HapticSettings();
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public void Save(string name, HapticSettings settings)
    {
        File.WriteAllText(PathFor(name), JsonSerializer.Serialize(settings, JsonOptions));
    }

    private string PathFor(string name)
    {
        var safe = string.Concat(name.Where(ch => !Path.GetInvalidFileNameChars().Contains(ch)));
        if (string.IsNullOrWhiteSpace(safe))
        {
            safe = "Custom";
        }

        return Path.Combine(_directory, safe + ".json");
    }

    private void EnsureBuiltInProfiles()
    {
        WriteIfMissing("Default", new HapticSettings());
        WriteIfMissing("Subtle", new HapticSettings
        {
            GlobalGain = 0.75f,
            MaximumRumble = 0.22f,
            EngineBase = 0.02f,
            EngineGain = 0.14f,
            RoadGain = 0.08f,
            SlipGain = 0.18f,
            DriftGain = 0.20f,
            AbsGain = 0.28f,
            TractionGain = 0.20f,
            ImpactGain = 0.60f,
            GForceGain = 0.06f
        });
        WriteIfMissing("Strong", new HapticSettings
        {
            GlobalGain = 1.15f,
            MaximumRumble = 0.55f,
            EngineBase = 0.045f,
            EngineGain = 0.30f,
            RoadGain = 0.20f,
            SlipGain = 0.38f,
            DriftGain = 0.42f,
            AbsGain = 0.55f,
            TractionGain = 0.40f,
            ImpactGain = 1.0f,
            GForceGain = 0.16f
        });
        WriteIfMissing("Race", new HapticSettings
        {
            GlobalGain = 1.0f,
            MaximumRumble = 0.40f,
            EngineBase = 0.025f,
            EngineGain = 0.20f,
            EngineExponent = 1.8f,
            RoadGain = 0.10f,
            SlipGain = 0.34f,
            DriftGain = 0.24f,
            AbsGain = 0.50f,
            TractionGain = 0.36f,
            GearShiftGain = 0.44f,
            ImpactGain = 0.80f,
            GForceEnabled = true,
            GForceGain = 0.12f
        });
        WriteIfMissing("Drift", new HapticSettings
        {
            GlobalGain = 1.05f,
            MaximumRumble = 0.48f,
            EngineBase = 0.028f,
            EngineGain = 0.18f,
            RoadGain = 0.10f,
            SlipGain = 0.36f,
            DriftGain = 0.55f,
            DriftThreshold = 0.14f,
            TractionGain = 0.22f,
            AbsGain = 0.30f,
            GForceGain = 0.18f
        });
    }

    private void WriteIfMissing(string name, HapticSettings settings)
    {
        var path = PathFor(name);
        if (!File.Exists(path))
        {
            File.WriteAllText(path, JsonSerializer.Serialize(settings, JsonOptions));
        }
    }
}
