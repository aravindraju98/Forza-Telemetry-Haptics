using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace ForzaTelemetryHaptics.App;

internal sealed class UpdateService
{
    private const string LatestUrl = "https://api.github.com/repos/aravindraju98/Forza-Telemetry-Haptics/releases/latest";
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(20)
    };

    static UpdateService()
    {
        Http.DefaultRequestHeaders.UserAgent.ParseAdd("ForzaTelemetryHaptics");
        Http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
    }

    public static Version CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);

    public static async Task<UpdateOffer?> CheckAsync()
    {
        using var response = await Http.GetAsync(LatestUrl);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;
        var tag = root.GetProperty("tag_name").GetString() ?? "";
        if (!TryParseTag(tag, out var remote) || Normalize(remote) <= Normalize(CurrentVersion))
        {
            return null;
        }

        string? url = null;
        if (root.TryGetProperty("assets", out var assets))
        {
            foreach (var asset in assets.EnumerateArray())
            {
                var name = asset.GetProperty("name").GetString() ?? "";
                if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    url = asset.GetProperty("browser_download_url").GetString();
                    break;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        return new UpdateOffer(tag, remote, url);
    }

    public static async Task ApplyAsync(UpdateOffer offer, string installDirectory)
    {
        var work = Path.Combine(Path.GetTempPath(), "ForzaTelemetryHaptics-update");
        if (Directory.Exists(work))
        {
            Directory.Delete(work, true);
        }

        Directory.CreateDirectory(work);
        var zip = Path.Combine(work, "update.zip");
        await using (var input = await Http.GetStreamAsync(offer.DownloadUrl))
        await using (var output = File.Create(zip))
        {
            await input.CopyToAsync(output);
        }

        var extracted = Path.Combine(work, "extracted");
        ZipFile.ExtractToDirectory(zip, extracted);
        var payload = FindPayload(extracted) ?? extracted;
        var updater = Path.Combine(work, "apply.cmd");
        await File.WriteAllTextAsync(updater, """
            @echo off
            setlocal
            set PID=%1
            set SRC=%~2
            set DST=%~3
            :wait
            tasklist /fi "PID eq %PID%" 2>nul | find "%PID%" >nul
            if not errorlevel 1 (
              timeout /t 1 /nobreak >nul
              goto wait
            )
            robocopy "%SRC%" "%DST%" /E /XF config.json /XD profiles
            start "" "%DST%\ForzaTelemetryHaptics.exe"
            """);

        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c \"{updater}\" {Environment.ProcessId} \"{payload}\" \"{installDirectory}\"",
            CreateNoWindow = true,
            UseShellExecute = false
        });
    }

    public static bool TryParseTag(string tag, out Version version)
    {
        var trimmed = tag.Trim();
        if (trimmed.StartsWith('v') || trimmed.StartsWith('V'))
        {
            trimmed = trimmed[1..];
        }

        return Version.TryParse(trimmed, out version!);
    }

    public static Version Normalize(Version version) =>
        new(version.Major, version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0));

    private static string? FindPayload(string extracted)
    {
        if (File.Exists(Path.Combine(extracted, "ForzaTelemetryHaptics.exe")))
        {
            return extracted;
        }

        foreach (var dir in Directory.GetDirectories(extracted))
        {
            if (File.Exists(Path.Combine(dir, "ForzaTelemetryHaptics.exe")))
            {
                return dir;
            }
        }

        return null;
    }
}

internal sealed record UpdateOffer(string Tag, Version Version, string DownloadUrl);
