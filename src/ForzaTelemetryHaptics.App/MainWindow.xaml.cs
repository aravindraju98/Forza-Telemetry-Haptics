using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ForzaTelemetryHaptics.Haptics;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.App;

public partial class MainWindow : Window
{
    private readonly AppRuntime _runtime;
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(70) };
    private readonly DispatcherTimer _saveTimer = new() { Interval = TimeSpan.FromMilliseconds(350) };
    private bool _ready;
    private string _selectedAspect = "Engine";

    public MainWindow()
    {
        InitializeComponent();
        SourceInitialized += (_, _) => WindowTheme.Apply(this);
        var root = AppContext.BaseDirectory;
        _runtime = new AppRuntime(
            Path.Combine(root, "config.json"),
            Path.Combine(root, "profiles"));

        FanCurve.CurveChanged += FanCurve_Changed;
        Closed += (_, _) =>
        {
            _timer.Stop();
            _saveTimer.Stop();
            try { _runtime.SaveConfig(); } catch { /* shutdown */ }
            _runtime.Stop();
        };

        _runtime.Start();
        BindAddressBox.Text = _runtime.Config.TelemetryBindAddress;
        PortBox.Text = _runtime.Config.TelemetryPort.ToString();
        RefreshProfiles();
        LoadCurvesToUi();

        _saveTimer.Tick += (_, _) =>
        {
            _saveTimer.Stop();
            _runtime.SaveConfig();
        };
        _timer.Tick += (_, _) => RefreshUi();
        _timer.Start();
        AutoUpdateBox.IsChecked = _runtime.Config.CheckForUpdates;
        _ready = true;
        UpdateSafetyLabels();
        UpdateCurveLabels();
        Loaded += (_, _) => _ = CheckForUpdateAsync();
    }

    private void RefreshUi()
    {
        var snap = _runtime.Snapshot();
        var tel = snap.Telemetry;
        var car = tel.Telemetry;
        var live = tel.State == TelemetryLinkState.Live;

        TelLink.Text = live ? "LIVE" : tel.State.ToString().ToUpperInvariant();
        TelLink.Foreground = Brush(live ? "Good" : "Muted");
        TelRate.Text = $"{tel.PacketRateHz} Hz";
        TelRpm.Text = live || car.Rpm > 0 ? $"{car.Rpm:0}" : "—";
        TelSpeed.Text = $"{car.SpeedKmh:0}";
        TelGear.Text = string.IsNullOrWhiteSpace(car.GearLabel) ? "—" : car.GearLabel;
        TelError.Text = tel.LastError;
        TelError.Visibility = string.IsNullOrWhiteSpace(tel.LastError)
            ? Visibility.Collapsed
            : Visibility.Visible;
        BindStatusText.Text = string.IsNullOrWhiteSpace(tel.BindEndpoint) ? "Starting" : tel.BindEndpoint;

        var pad = snap.Controller;
        PadStatus.Text = pad.Detected
            ? $"{pad.ControllerType} · {pad.Index} · {(pad.RumbleAvailable ? "rumble" : "no rumble")}"
            : "No controller";
        PadError.Text = pad.Error;
        PadError.Visibility = string.IsNullOrWhiteSpace(pad.Error)
            ? Visibility.Collapsed
            : Visibility.Visible;

        var hap = snap.Haptics;
        SetBar(HapEngine, BarEngine, hap.Engine);
        SetBar(HapBoost, BarBoost, hap.Boost);
        SetBar(HapRoad, BarRoad, hap.Road);
        SetBar(HapSlip, BarSlip, hap.Slip);
        SetBar(HapAbs, BarAbs, hap.Abs);
        SetBar(HapTraction, BarTraction, hap.Traction);
        SetBar(HapHandbrake, BarHandbrake, hap.Handbrake);
        SetBar(HapGear, BarGear, hap.GearShift);
        SetBar(HapImpact, BarImpact, hap.Impact);
        HapLeft.Text = $"{hap.LeftMotor * 100:0.0}";
        HapRight.Text = $"{hap.RightMotor * 100:0.0}";
        BarLeft.Value = hap.LeftMotor;
        BarRight.Value = hap.RightMotor;
        UpdateLiveCurveMarker(car, hap);
    }

    private static void SetBar(System.Windows.Controls.TextBlock label, System.Windows.Controls.ProgressBar bar, float value)
    {
        label.Text = $"{value * 100:0}";
        bar.Value = Math.Clamp(value, 0f, 1f);
    }

    private System.Windows.Media.Brush Brush(string key) =>
        (System.Windows.Media.Brush)FindResource(key);

    private void TestLeft_Click(object sender, RoutedEventArgs e) => _runtime.TestMotor(0.35f, 0f);
    private void TestRight_Click(object sender, RoutedEventArgs e) => _runtime.TestMotor(0f, 0.35f);
    private void TestBoth_Click(object sender, RoutedEventArgs e) => _runtime.TestMotor(0.28f, 0.28f);
    private void StopRumble_Click(object sender, RoutedEventArgs e) => _runtime.TestMotor(0f, 0f, 50);

    private void LoadProfile_Click(object sender, RoutedEventArgs e)
    {
        if (ProfileBox.SelectedItem is not string name) return;
        var error = _runtime.LoadProfile(name);
        if (!string.IsNullOrEmpty(error))
        {
            MessageBox.Show(error, "Profile", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        _ready = false;
        LoadCurvesToUi();
        _ready = true;
        UpdateSafetyLabels();
        UpdateCurveLabels();
    }

    private void SaveProfile_Click(object sender, RoutedEventArgs e)
    {
        var name = string.IsNullOrWhiteSpace(NewProfileName.Text) ? "Custom" : NewProfileName.Text.Trim();
        _runtime.SaveCurrentAsProfile(name);
        RefreshProfiles();
        ProfileBox.SelectedItem = name;
    }

    private void RestartTelemetry_Click(object sender, RoutedEventArgs e)
    {
        _runtime.Config.TelemetryBindAddress = BindAddressBox.Text.Trim();
        if (int.TryParse(PortBox.Text, out var port))
        {
            _runtime.Config.TelemetryPort = port;
        }

        _runtime.SaveConfig();
        _runtime.RestartTelemetry();
    }

    private void MaxRumble_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_ready) return;
        _runtime.Config.Haptics.MaximumRumble = (float)MaxRumbleSlider.Value;
        UpdateSafetyLabels();
        ScheduleSave();
    }

    private void GlobalGain_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_ready) return;
        _runtime.Config.Haptics.GlobalGain = (float)GlobalGainSlider.Value;
        UpdateSafetyLabels();
        ScheduleSave();
    }

    private void AutoUpdate_Changed(object sender, RoutedEventArgs e)
    {
        if (!_ready) return;
        _runtime.Config.CheckForUpdates = AutoUpdateBox.IsChecked == true;
        ScheduleSave();
        if (_runtime.Config.CheckForUpdates)
        {
            _ = CheckForUpdateAsync();
        }
    }

    private async Task CheckForUpdateAsync()
    {
        if (!_runtime.Config.CheckForUpdates)
        {
            return;
        }

        try
        {
            var offer = await UpdateService.CheckAsync();
            if (offer is null)
            {
                return;
            }

            var go = MessageBox.Show(
                this,
                $"Version {offer.Tag} is available (you have {UpdateService.CurrentVersion}).\n\nInstall it now? Your config and profiles stay.",
                "Update",
                MessageBoxButton.YesNo,
                MessageBoxImage.None);
            if (go != MessageBoxResult.Yes)
            {
                return;
            }

            await UpdateService.ApplyAsync(offer, AppContext.BaseDirectory);
            ForceSilence();
            Close();
        }
        catch
        {
            // offline or GitHub unreachable
        }
    }

    private void LeftMotorScale_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_ready) return;
        _runtime.Config.Haptics.LeftMotorScale = (float)LeftMotorScaleSlider.Value;
        UpdateSafetyLabels();
        ScheduleSave();
    }

    private void Curve_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) => OnCurveEdited();

    private void OnCurveEdited()
    {
        if (!_ready) return;
        ApplyCurvesFromUi();
        UpdateCurveLabels();
        ScheduleSave();
    }

    private void ScheduleSave()
    {
        _saveTimer.Stop();
        _saveTimer.Start();
    }

    private void LoadCurvesToUi()
    {
        var h = _runtime.Config.Haptics;
        MaxRumbleSlider.Value = h.MaximumRumble;
        GlobalGainSlider.Value = h.GlobalGain;
        LeftMotorScaleSlider.Value = h.LeftMotorScale;
        EngineBaseSlider.Value = h.EngineBase;
        EngineGainSlider.Value = h.EngineGain;
        EngineExponentSlider.Value = h.EngineExponent;
        EngineModSlider.Value = h.EngineModulation;
        EngineModHzSlider.Value = h.EngineModulationHz;
        EngineRedlineHzSlider.Value = h.EngineRedlineModHz;
        EngineThrottleSlider.Value = h.EngineThrottleBlend;
        BoostGainSlider.Value = h.BoostGain;
        RoadGainSlider.Value = h.RoadGain;
        RoadSurfaceSlider.Value = h.RoadSurfaceGain;
        RoadStripSlider.Value = h.RoadRumbleStripGain;
        RoadPuddleSlider.Value = h.RoadPuddleGain;
        HandbrakeGainSlider.Value = h.HandbrakeGain;
        SlipGainSlider.Value = h.SlipGain;
        WheelspinSlider.Value = h.WheelspinGain;
        DriftGainSlider.Value = h.DriftGain;
        DriftThresholdSlider.Value = h.DriftThreshold;
        AbsGainSlider.Value = h.AbsGain;
        AbsHzSlider.Value = h.AbsFrequencyHz;
        AbsWidthSlider.Value = h.AbsPulseWidth;
        TractionGainSlider.Value = h.TractionGain;
        TractionHzSlider.Value = h.TractionFrequencyHz;
        GearGainSlider.Value = h.GearShiftGain;
        GearMsSlider.Value = h.GearShiftDurationMs;
        ImpactGainSlider.Value = h.ImpactGain;
        ImpactDecaySlider.Value = h.ImpactDecayMs;
        GForceGainSlider.Value = h.GForceGain;
        foreach (var (_, name, toggle, _) in LayerRows())
        {
            toggle.IsChecked = h.IsEnabled(name);
        }

        ShowSelectedCurve();
        HighlightLayers();
    }

    private void ApplyCurvesFromUi()
    {
        var h = _runtime.Config.Haptics;
        h.EngineBase = (float)EngineBaseSlider.Value;
        h.EngineGain = (float)EngineGainSlider.Value;
        h.EngineExponent = (float)EngineExponentSlider.Value;
        h.EngineModulation = (float)EngineModSlider.Value;
        h.EngineModulationHz = (float)EngineModHzSlider.Value;
        h.EngineRedlineModHz = (float)EngineRedlineHzSlider.Value;
        h.EngineThrottleBlend = (float)EngineThrottleSlider.Value;
        h.BoostGain = (float)BoostGainSlider.Value;
        h.RoadGain = (float)RoadGainSlider.Value;
        h.RoadSurfaceGain = (float)RoadSurfaceSlider.Value;
        h.RoadRumbleStripGain = (float)RoadStripSlider.Value;
        h.RoadPuddleGain = (float)RoadPuddleSlider.Value;
        h.HandbrakeGain = (float)HandbrakeGainSlider.Value;
        h.SlipGain = (float)SlipGainSlider.Value;
        h.WheelspinGain = (float)WheelspinSlider.Value;
        h.DriftGain = (float)DriftGainSlider.Value;
        h.DriftThreshold = (float)DriftThresholdSlider.Value;
        h.AbsGain = (float)AbsGainSlider.Value;
        h.AbsFrequencyHz = (float)AbsHzSlider.Value;
        h.AbsPulseWidth = (float)AbsWidthSlider.Value;
        h.TractionGain = (float)TractionGainSlider.Value;
        h.TractionFrequencyHz = (float)TractionHzSlider.Value;
        h.GearShiftGain = (float)GearGainSlider.Value;
        h.GearShiftDurationMs = (float)GearMsSlider.Value;
        h.ImpactGain = (float)ImpactGainSlider.Value;
        h.ImpactDecayMs = (float)ImpactDecaySlider.Value;
        h.GForceGain = (float)GForceGainSlider.Value;
    }

    private void UpdateSafetyLabels()
    {
        MaxRumbleText.Text = $"{_runtime.Config.Haptics.MaximumRumble * 100:0}%";
        GlobalGainText.Text = $"{_runtime.Config.Haptics.GlobalGain * 100:0}%";
        LeftMotorScaleText.Text = $"{_runtime.Config.Haptics.LeftMotorScale * 100:0}%";
    }

    private void UpdateCurveLabels()
    {
        var h = _runtime.Config.Haptics;
        LblEngineBase.Text = $"Idle base  {h.EngineBase * 100:0.0}";
        LblEngineGain.Text = $"RPM gain  {h.EngineGain * 100:0}";
        LblEngineExponent.Text = $"RPM curve  {h.EngineExponent:0.00}";
        LblEngineMod.Text = $"Texture  {h.EngineModulation * 100:0}";
        LblEngineModHz.Text = $"Idle texture  {h.EngineModulationHz:0} Hz";
        LblEngineRedlineHz.Text = $"Redline texture  {h.EngineRedlineModHz:0} Hz";
        LblEngineThrottle.Text = $"Throttle blend  {h.EngineThrottleBlend * 100:0}";
        LblBoostGain.Text = $"Boost  {h.BoostGain * 100:0}";
        LblRoadGain.Text = $"Speed  {h.RoadGain * 100:0}";
        LblRoadSurface.Text = $"Surface  {h.RoadSurfaceGain * 100:0}";
        LblRoadStrip.Text = $"Rumble strip  {h.RoadRumbleStripGain * 100:0}";
        LblRoadPuddle.Text = $"Puddle  {h.RoadPuddleGain * 100:0}";
        LblHandbrakeGain.Text = $"Grab  {h.HandbrakeGain * 100:0}";
        LblSlipGain.Text = $"Slip  {h.SlipGain * 100:0}";
        LblWheelspin.Text = $"Wheelspin  {h.WheelspinGain * 100:0}";
        LblDriftGain.Text = $"Drift  {h.DriftGain * 100:0}";
        LblDriftThreshold.Text = $"Drift start  {h.DriftThreshold * 100:0}";
        LblAbsGain.Text = $"Gain  {h.AbsGain * 100:0}";
        LblAbsHz.Text = $"Pulse  {h.AbsFrequencyHz:0} Hz";
        LblAbsWidth.Text = $"Width  {h.AbsPulseWidth * 100:0}";
        LblTractionGain.Text = $"Gain  {h.TractionGain * 100:0}";
        LblTractionHz.Text = $"Pulse  {h.TractionFrequencyHz:0} Hz";
        LblGearGain.Text = $"Kick  {h.GearShiftGain * 100:0}";
        LblGearMs.Text = $"Duration  {h.GearShiftDurationMs:0} ms";
        LblImpactGain.Text = $"Gain  {h.ImpactGain * 100:0}";
        LblImpactDecay.Text = $"Decay  {h.ImpactDecayMs:0} ms";
        LblGForceGain.Text = $"Gain  {h.GForceGain * 100:0}";
    }

    private void LayerRow_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string aspect })
        {
            SelectAspect(aspect);
        }
    }

    private void LayerToggle_Changed(object sender, RoutedEventArgs e)
    {
        if (!_ready || sender is not CheckBox { Tag: string aspect } box)
        {
            return;
        }

        _runtime.Config.Haptics.SetEnabled(aspect, box.IsChecked == true);
        ScheduleSave();
    }

    private void SelectAspect(string aspect)
    {
        _selectedAspect = aspect;
        HighlightLayers();
        ShowSelectedCurve();
    }

    private void HighlightLayers()
    {
        foreach (var (row, name, _, label) in LayerRows())
        {
            var selected = name == _selectedAspect;
            row.Background = selected ? (Brush)FindResource("PanelAlt") : Brushes.Transparent;
            label.Foreground = selected ? (Brush)FindResource("Accent") : (Brush)FindResource("Muted");
        }
    }

    private IEnumerable<(Border Row, string Name, CheckBox Toggle, TextBlock Label)> LayerRows()
    {
        yield return (RowEngine, "Engine", EngineEnabledBox, LblLayerEngine);
        yield return (RowBoost, "Boost", BoostEnabledBox, LblLayerBoost);
        yield return (RowRoad, "Road", RoadEnabledBox, LblLayerRoad);
        yield return (RowSlip, "Slip", SlipEnabledBox, LblLayerSlip);
        yield return (RowDrift, "Drift", DriftEnabledBox, LblLayerDrift);
        yield return (RowAbs, "ABS", AbsEnabledBox, LblLayerAbs);
        yield return (RowTraction, "Traction", TractionEnabledBox, LblLayerTraction);
        yield return (RowHandbrake, "Handbrake", HandbrakeEnabledBox, LblLayerHandbrake);
        yield return (RowGear, "Gear", GearEnabledBox, LblLayerGear);
        yield return (RowImpact, "Impact", ImpactEnabledBox, LblLayerImpact);
        yield return (RowGForce, "G-force", GForceEnabledBox, LblLayerGForce);
    }

    private void FanCurve_Changed(object? sender, EventArgs e)
    {
        if (!_ready)
        {
            return;
        }

        _runtime.Config.Haptics.SetCurve(_selectedAspect, FanCurve.Curve);
        ScheduleSave();
    }

    private void ResetCurve_Click(object sender, RoutedEventArgs e)
    {
        var aspect = _selectedAspect;
        var fresh = new ForzaTelemetryHaptics.Configuration.HapticSettings();
        FanCurve.ResetTo(fresh.CurveFor(aspect).Clone());
        _runtime.Config.Haptics.SetCurve(aspect, FanCurve.Curve);
        ScheduleSave();
    }

    private void ShowSelectedCurve()
    {
        var aspect = _selectedAspect;
        FanCurve.Curve = _runtime.Config.Haptics.CurveFor(aspect);
        FanCurve.AxisY = "Rumble";
        FanCurve.AxisX = aspect switch
        {
            "Road" => "Speed",
            "Slip" => "Tire slip",
            "Drift" => "Drift amount",
            "ABS" => "ABS strength",
            "Traction" => "Traction strength",
            "Gear" => "Shift amount",
            "Impact" => "Impact size",
            "G-force" => "G-force",
            "Handbrake" => "Drift amount",
            "Boost" => "RPM (idle → redline)",
            _ => "RPM (idle → redline)"
        };
        CurveHelp.Text = $"{aspect}: {FanCurve.AxisX} → rumble";
        ShowSelectedSliders(aspect);
    }

    private void ShowSelectedSliders(string aspect)
    {
        PanelEngine.Visibility = aspect is "Engine" or "Boost" ? Visibility.Visible : Visibility.Collapsed;
        PanelRoad.Visibility = aspect == "Road" ? Visibility.Visible : Visibility.Collapsed;
        PanelSlip.Visibility = aspect == "Slip" ? Visibility.Visible : Visibility.Collapsed;
        PanelDrift.Visibility = aspect is "Drift" or "Handbrake" ? Visibility.Visible : Visibility.Collapsed;
        PanelAbs.Visibility = aspect == "ABS" ? Visibility.Visible : Visibility.Collapsed;
        PanelTraction.Visibility = aspect == "Traction" ? Visibility.Visible : Visibility.Collapsed;
        PanelGear.Visibility = aspect == "Gear" ? Visibility.Visible : Visibility.Collapsed;
        PanelImpact.Visibility = aspect == "Impact" ? Visibility.Visible : Visibility.Collapsed;
        PanelGForce.Visibility = aspect == "G-force" ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateLiveCurveMarker(VehicleTelemetry car, HapticDebugState hap)
    {
        var aspect = _selectedAspect;
        var x = aspect switch
        {
            "Road" => car.NormalizedSpeed,
            "Slip" => car.AggregateSlip,
            "Drift" or "Handbrake" => car.RearSlip,
            "ABS" => car.AbsStrength,
            "Traction" => car.TractionStrength,
            "Gear" => hap.GearShift > 0.05f ? 0.7f : 0f,
            "Impact" => car.ImpactMagnitude,
            "G-force" => Math.Clamp(Math.Max(Math.Abs(car.AccelLateral), Math.Abs(car.AccelLongitudinal)) / 16f, 0f, 1f),
            _ => car.NormalizedRpm
        };
        FanCurve.SetLiveInput(x);
    }

    public void ForceSilence()
    {
        try
        {
            _runtime.TestMotor(0f, 0f, 10);
            _runtime.Stop();
        }
        catch
        {
            // shutdown path
        }
    }

    private void RefreshProfiles()
    {
        ProfileBox.Items.Clear();
        foreach (var name in _runtime.Profiles)
        {
            ProfileBox.Items.Add(name);
        }

        ProfileBox.SelectedItem = _runtime.Config.ActiveProfile;
    }
}
