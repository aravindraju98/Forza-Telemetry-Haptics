using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Controllers;
using ForzaTelemetryHaptics.Haptics;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics;

public sealed class AppRuntime : IDisposable
{
    private readonly object _gate = new();
    private readonly ConfigManager _configManager;
    private readonly ProfileManager _profileManager;
    private readonly TelemetryReceiver _receiver;
    private readonly TelemetryNormalizer _normalizer = new();
    private readonly HapticEngine _engine = new();
    private IControllerHaptics _controller;
    private readonly bool _ownsController;
    private CancellationTokenSource? _cts;
    private Task? _loop;
    private HapticSignal _manualOverride;
    private bool _manualActive;
    private DateTime _manualUntil;
    private UiSnapshot _ui = new();

    public AppRuntime(string configPath, string profilesDirectory, IControllerHaptics? controller = null)
    {
        _configManager = new ConfigManager(configPath);
        _profileManager = new ProfileManager(profilesDirectory);
        _receiver = new TelemetryReceiver(new Fh6TelemetryParser(), _normalizer);
        _ownsController = controller is null;
        _controller = controller ?? new XInputController();
    }

    public AppConfig Config => _configManager.Current;
    public IReadOnlyList<string> Profiles => _profileManager.ListProfiles();

    public void Start()
    {
        _configManager.LoadOrCreate();
        ApplyStoredProfile();
        if (_ownsController)
        {
            _controller.Dispose();
            _controller = new XInputController(Config.ControllerIndex >= 0 ? Config.ControllerIndex : null);
        }

        _controller.Connect();
        _receiver.Start(Config.TelemetryBindAddress, Config.TelemetryPort);
        _cts = new CancellationTokenSource();
        _loop = Task.Run(() => ProcessLoop(_cts.Token));
    }

    public void Stop()
    {
        try { _cts?.Cancel(); } catch { /* ignored */ }
        try { _loop?.Wait(TimeSpan.FromMilliseconds(500)); } catch { /* ignored */ }
        _receiver.Stop();
        _controller.StopRumble();
        _controller.Disconnect();
        _cts?.Dispose();
        _cts = null;
        _loop = null;
    }

    public UiSnapshot Snapshot()
    {
        lock (_gate)
        {
            return _ui;
        }
    }

    public void TestMotor(float left, float right, int durationMs = 700)
    {
        lock (_gate)
        {
            _manualOverride = new HapticSignal(left, right);
            _manualActive = true;
            _manualUntil = DateTime.UtcNow.AddMilliseconds(durationMs);
        }
    }

    public string LoadProfile(string name)
    {
        if (!_profileManager.TryLoad(name, out var settings, out var error))
        {
            return error;
        }

        _configManager.ReplaceHaptics(settings);
        Config.ActiveProfile = name;
        _configManager.Save();
        return string.Empty;
    }

    public void SaveCurrentAsProfile(string name)
    {
        _profileManager.Save(name, Config.Haptics);
        Config.ActiveProfile = name;
        _configManager.Save();
    }

    public void SaveConfig() => _configManager.Save();

    public void RestartTelemetry()
    {
        _receiver.Stop();
        _receiver.Start(Config.TelemetryBindAddress, Config.TelemetryPort);
    }

    private void ApplyStoredProfile()
    {
        if (_profileManager.TryLoad(Config.ActiveProfile, out var settings, out _))
        {
            _configManager.ReplaceHaptics(settings);
        }
    }

    private void ProcessLoop(CancellationToken token)
    {
        var interval = TimeSpan.FromSeconds(1.0 / Math.Max(Config.HapticRateHz, 20));
        var last = DateTime.UtcNow;
        while (!token.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var dt = (float)(now - last).TotalSeconds;
            last = now;
            Tick(dt, now);
            try
            {
                Task.Delay(interval, token).Wait(token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _controller.StopRumble();
    }

    private void Tick(float deltaSeconds, DateTime now)
    {
        var staleAfter = TimeSpan.FromMilliseconds(Config.Haptics.TelemetryTimeoutMs);
        var telemetrySnap = _receiver.Snapshot(staleAfter);
        var telemetry = telemetrySnap.Telemetry;
        var live = telemetrySnap.State == TelemetryLinkState.Live
                   && (telemetry.IsDriving || telemetry.EngineRunning || telemetry.Rpm > 200f || telemetry.SpeedMps > 0.35f);

        HapticSignal output;
        lock (_gate)
        {
            if (_manualActive)
            {
                if (now <= _manualUntil)
                {
                    output = _manualOverride;
                }
                else
                {
                    _manualActive = false;
                    output = _engine.Update(telemetry, Config.Haptics, deltaSeconds, live);
                }
            }
            else
            {
                output = _engine.Update(telemetry, Config.Haptics, deltaSeconds, live);
            }
        }

        var status = _controller.GetControllerStatus();
        if (status.Detected)
        {
            _controller.SetRumble(output.LeftMotor, output.RightMotor);
        }

        lock (_gate)
        {
            _ui = new UiSnapshot
            {
                Telemetry = telemetrySnap,
                Controller = status,
                Haptics = _engine.LastDebug with { LeftMotor = output.LeftMotor, RightMotor = output.RightMotor },
                ActiveProfile = Config.ActiveProfile
            };
        }
    }

    public void Dispose() => Stop();
}

public sealed class UiSnapshot
{
    public TelemetrySnapshot Telemetry { get; init; } = new();
    public ControllerStatus Controller { get; init; } = new();
    public HapticDebugState Haptics { get; init; } = new();
    public string ActiveProfile { get; init; } = "Default";
}
