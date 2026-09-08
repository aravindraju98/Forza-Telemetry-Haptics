using ForzaTelemetryHaptics.Configuration;
using ForzaTelemetryHaptics.Controllers;
using ForzaTelemetryHaptics.Haptics;
using ForzaTelemetryHaptics.Haptics.Effects;
using ForzaTelemetryHaptics.Haptics.Filters;
using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Tests;

public class HapticEngineTests
{
    [Fact]
    public void Mixer_ClampsToMaximumRumble()
    {
        var settings = new HapticSettings { GlobalGain = 2f, MaximumRumble = 0.35f, MinimumThreshold = 0f, ImpactPriority = 0f };
        var loud = new HapticSignal(1f, 1f);
        var mixed = new HapticMixer().Mix(loud, loud, loud, loud, loud, loud, loud, loud, loud, HapticSignal.Zero, settings);
        Assert.Equal(0.35f, mixed.LeftMotor, 3);
        Assert.Equal(0.35f, mixed.RightMotor, 3);
    }

    [Fact]
    public void Mixer_LeftScaleSoftensHeavierMotor()
    {
        var settings = new HapticSettings
        {
            GlobalGain = 1f,
            MaximumRumble = 1f,
            MinimumThreshold = 0f,
            ImpactPriority = 0f,
            LeftMotorScale = 0.8f,
            RightMotorScale = 1f
        };
        var even = new HapticSignal(0.4f, 0.4f);
        var mixed = new HapticMixer().Mix(even, HapticSignal.Zero, HapticSignal.Zero, HapticSignal.Zero, HapticSignal.Zero, HapticSignal.Zero, HapticSignal.Zero, HapticSignal.Zero, HapticSignal.Zero, HapticSignal.Zero, settings);
        Assert.InRange(mixed.LeftMotor, 0.30f, 0.33f);
        Assert.InRange(mixed.RightMotor, 0.39f, 0.41f);
    }

    [Fact]
    public void Mixer_ImpactCanDominateBaseline()
    {
        var settings = new HapticSettings { GlobalGain = 1f, MaximumRumble = 1f, MinimumThreshold = 0f, ImpactPriority = 1f, LeftMotorScale = 1f, RightMotorScale = 1f };
        var engine = new HapticSignal(0.2f, 0.2f);
        var impact = new HapticSignal(0.9f, 0.9f);
        var mixed = new HapticMixer().Mix(engine, HapticSignal.Zero, HapticSignal.Zero, HapticSignal.Zero, HapticSignal.Zero, HapticSignal.Zero, HapticSignal.Zero, HapticSignal.Zero, HapticSignal.Zero, impact, settings);
        Assert.InRange(mixed.LeftMotor, 0.85f, 1f);
    }

    [Fact]
    public void Smoothing_MovesTowardTargetWithoutJumping()
    {
        var filter = new LowPassFilter();
        var a = filter.Update(1f, 0.2f, 0.01f);
        Assert.InRange(a, 0.01f, 0.2f);
        var b = filter.Update(1f, 0.2f, 0.01f);
        Assert.True(b > a);
    }

    [Fact]
    public void GearChange_ProducesTransientThenSettles()
    {
        var effect = new GearShiftEffect();
        var settings = new HapticSettings { GearShiftGain = 0.5f, GearShiftDurationMs = 80 };
        var first = Driving(gear: 3);
        effect.Update(first, settings, 0.1f);
        var kick = effect.Update(Driving(gear: 4), settings, 0.1f);
        Assert.True(kick.Peak > 0.2f);
        HapticSignal last = kick;
        for (var i = 0; i < 30; i++)
        {
            last = effect.Update(Driving(gear: 4), settings, 0.02f);
        }

        Assert.True(last.Peak < 0.05f);
    }

    [Fact]
    public void AbsPulse_IsIntermittentNotContinuous()
    {
        var pulse = new PulseGenerator();
        var highs = 0;
        var lows = 0;
        for (var i = 0; i < 40; i++)
        {
            var value = pulse.Update(true, 10f, 0.4f, 0.01f);
            if (value > 0.5f) highs++;
            else lows++;
        }

        Assert.True(highs > 5);
        Assert.True(lows > 5);
    }

    [Fact]
    public void ImpactEnvelope_DecaysAndDoesNotStick()
    {
        var effect = new ImpactEffect();
        var settings = new HapticSettings { ImpactGain = 1f, ImpactAttackMs = 10, ImpactDecayMs = 80 };
        var hit = Driving(impact: 0.8f);
        var peak = effect.Update(hit, settings, 0.01f);
        Assert.True(peak.Peak > 0.3f);
        var quiet = Driving(impact: 0f);
        HapticSignal last = peak;
        for (var i = 0; i < 40; i++)
        {
            last = effect.Update(quiet, settings, 0.02f);
        }

        Assert.True(last.Peak < 0.05f);
    }

    [Fact]
    public void Engine_IsPresentAtIdleAndStrongerAtRedline()
    {
        var effect = new EngineEffect();
        var settings = new HapticSettings();
        var idle = effect.Update(Driving(rpm: 900, max: 7500, idleRpm: 800), settings, 0.05f);
        effect.Reset();
        var redline = effect.Update(Driving(rpm: 7400, max: 7500, idleRpm: 800), settings, 0.05f);
        Assert.True(idle.Peak > 0f);
        Assert.True(redline.Peak > idle.Peak);
        Assert.True(redline.Peak < 0.6f);
    }

    [Fact]
    public void Timeout_FadesRumbleToZero()
    {
        var engine = new HapticEngine();
        var settings = new HapticSettings { FadeOutSeconds = 0.05f, MaximumRumble = 1f };
        engine.Update(Driving(rpm: 4000), settings, 0.02f, live: true);
        HapticSignal last = default;
        for (var i = 0; i < 40; i++)
        {
            last = engine.Update(null, settings, 0.02f, live: false);
        }

        Assert.Equal(0f, last.LeftMotor, 3);
        Assert.Equal(0f, last.RightMotor, 3);
    }

    [Fact]
    public void ControllerDisconnect_StopsAcceptingRumble()
    {
        var pad = new FakeController();
        Assert.True(pad.SetRumble(0.2f, 0.2f));
        pad.Disconnect();
        Assert.False(pad.SetRumble(1f, 1f));
        Assert.Equal(0f, pad.LastLeft);
        Assert.False(pad.GetControllerStatus().Detected);
    }

    [Fact]
    public void SpeedImpact_ReachesMixerOutput()
    {
        var engine = new HapticEngine();
        var settings = new HapticSettings { MaximumRumble = 1f, MinimumThreshold = 0f, GlobalGain = 1f };
        engine.Update(Driving(rpm: 3000), settings, 0.02f, true);
        var hit = Driving(rpm: 3000, impact: 0.7f);
        var output = engine.Update(hit, settings, 0.02f, true);
        Assert.True(output.Peak > 0.4f);
        Assert.True(engine.LastDebug.Impact > 0.3f);
    }

    [Fact]
    public void Engine_IsQuieterOffThrottleThanOnThrottle()
    {
        var settings = new HapticSettings { EngineThrottleBlend = 0.8f, EngineGain = 0.3f };
        var coast = new EngineEffect().Update(Driving(rpm: 4000, throttle: 0f), settings, 0.05f);
        var onGas = new EngineEffect().Update(Driving(rpm: 4000, throttle: 1f), settings, 0.05f);
        Assert.True(onGas.Peak > coast.Peak);
        Assert.True(coast.Peak > 0f);
    }

    [Fact]
    public void Boost_AddsRightBiasedRumbleWhenActive()
    {
        var effect = new BoostEffect();
        var settings = new HapticSettings { BoostGain = 0.4f };
        var quiet = effect.Update(Driving(rpm: 4000), settings, 0.05f);
        var on = effect.Update(Driving(rpm: 4000, boost: 12f), settings, 0.05f);
        Assert.True(on.Peak > quiet.Peak);
        Assert.True(on.RightMotor > on.LeftMotor);
    }

    [Fact]
    public void Road_PansTowardTheSideWithSurfaceAndPuddle()
    {
        var effect = new RoadEffect();
        var settings = new HapticSettings { RoadGain = 0f, RoadSurfaceGain = 0.8f, RoadPuddleGain = 0.6f, RoadRumbleStripGain = 0f };
        var tel = Driving(rpm: 2500, speed: 25, leftSurface: 0.8f, rightSurface: 0.05f, puddleLeft: 0.5f, puddleRight: 0f);
        var signal = effect.Update(tel, settings, 0.05f);
        Assert.True(signal.LeftMotor > signal.RightMotor);
    }

    [Fact]
    public void Handbrake_RumblesWhileHeld()
    {
        var effect = new HandbrakeEffect();
        var settings = new HapticSettings { HandbrakeGain = 0.5f };
        var off = effect.Update(Driving(rpm: 2500, speed: 20), settings, 0.05f);
        var on = effect.Update(Driving(rpm: 2500, speed: 20, handbrake: 0.9f), settings, 0.05f);
        Assert.True(on.Peak > off.Peak);
        Assert.True(on.Peak > 0.2f);
    }

    [Fact]
    public void Handbrake_IsSilentWhenIdleAndStrongerWhenMoving()
    {
        var settings = new HapticSettings { HandbrakeGain = 0.5f };
        var idle = new HandbrakeEffect().Update(Driving(rpm: 900, speed: 0f, handbrake: 1f), settings, 0.05f);
        var slow = new HandbrakeEffect().Update(Driving(rpm: 2500, speed: 4f, handbrake: 1f), settings, 0.05f);
        var fast = new HandbrakeEffect().Update(Driving(rpm: 2500, speed: 20f, handbrake: 1f), settings, 0.05f);
        Assert.True(idle.Peak < 0.02f);
        Assert.True(fast.Peak > slow.Peak);
        Assert.True(slow.Peak > idle.Peak);
    }

    [Fact]
    public void SuspensionThud_TriggersDirectionalImpact()
    {
        var effect = new ImpactEffect();
        var settings = new HapticSettings { ImpactGain = 1f, ImpactAttackMs = 10, ImpactDecayMs = 80 };
        var tel = Driving(rpm: 2500, speed: 20, thudLeft: 0.7f, thudRight: 0.1f);
        var hit = effect.Update(tel, settings, 0.01f);
        Assert.True(hit.Peak > 0.3f);
        Assert.True(hit.LeftMotor > hit.RightMotor);
    }

    [Fact]
    public void DisabledLayer_DropsThatChannel()
    {
        var engine = new HapticEngine();
        var on = new HapticSettings { MaximumRumble = 1f, MinimumThreshold = 0f, GlobalGain = 1f };
        engine.Update(Driving(rpm: 5200, throttle: 1f), on, 0.05f, true);
        Assert.True(engine.LastDebug.Engine > 0.02f);

        engine.Reset();
        var off = new HapticSettings { MaximumRumble = 1f, MinimumThreshold = 0f, GlobalGain = 1f, EngineEnabled = false };
        engine.Update(Driving(rpm: 5200, throttle: 1f), off, 0.05f, true);
        Assert.Equal(0f, engine.LastDebug.Engine);
    }

    [Fact]
    public void Engine_RunsWithoutPhysicalController()
    {
        var engine = new HapticEngine();
        var output = engine.Update(Driving(rpm: 3000, speed: 20), new HapticSettings(), 0.01f, live: true);
        Assert.True(output.LeftMotor >= 0f);
        Assert.True(output.RightMotor >= 0f);
        Assert.True(output.Peak <= 1f);
    }

    private static VehicleTelemetry Driving(
        float rpm = 3000,
        float max = 7500,
        float idleRpm = 800,
        int gear = 3,
        float speed = 20,
        float impact = 0f,
        float throttle = 0.4f,
        float boost = 0f,
        float handbrake = 0f,
        float leftSurface = 0f,
        float rightSurface = 0f,
        float puddleLeft = 0f,
        float puddleRight = 0f,
        float thudLeft = 0f,
        float thudRight = 0f)
    {
        var normalizedBoost = boost > 1.5f ? MathUtil.Clamp01(boost / 18f) : MathUtil.Clamp01(boost);
        return new VehicleTelemetry
        {
            IsDriving = true,
            EngineRunning = true,
            Rpm = rpm,
            MaxRpm = max,
            IdleRpm = idleRpm,
            NormalizedRpm = MathUtil.Clamp01((rpm - idleRpm) / (max - idleRpm)),
            SpeedMps = speed,
            SpeedKmh = speed * 3.6f,
            NormalizedSpeed = MathUtil.Clamp01(speed / 83f),
            Gear = gear,
            Throttle = throttle,
            Handbrake = handbrake,
            Boost = boost,
            NormalizedBoost = normalizedBoost,
            BoostActive = normalizedBoost > 0.08f,
            SurfaceRumble = new WheelCorner<float>(leftSurface, rightSurface, leftSurface, rightSurface),
            PuddleLeft = puddleLeft,
            PuddleRight = puddleRight,
            SuspensionThudLeft = thudLeft,
            SuspensionThudRight = thudRight,
            ImpactMagnitude = Math.Max(impact, MathF.Max(thudLeft, thudRight)),
            SmashableImpact = impact,
            DrivetrainType = 1,
            DrivetrainLabel = "RWD"
        };
    }
}
