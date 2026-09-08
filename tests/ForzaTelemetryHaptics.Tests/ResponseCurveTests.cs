using ForzaTelemetryHaptics.Haptics;

namespace ForzaTelemetryHaptics.Tests;

public class ResponseCurveTests
{
    [Fact]
    public void Linear_PassesInputThrough()
    {
        var curve = ResponseCurve.Linear();
        Assert.Equal(0f, curve.Evaluate(0f), 3);
        Assert.Equal(0.5f, curve.Evaluate(0.5f), 3);
        Assert.Equal(1f, curve.Evaluate(1f), 3);
    }

    [Fact]
    public void PowerCurve_StaysBelowLinearInTheMiddle()
    {
        var curve = ResponseCurve.Power(1.6f);
        Assert.True(curve.Evaluate(0.5f) < 0.5f);
        Assert.Equal(1f, curve.Evaluate(1f), 3);
    }

    [Fact]
    public void DelayedCurve_IsQuietUntilStart()
    {
        var curve = ResponseCurve.Delayed(0.3f);
        Assert.Equal(0f, curve.Evaluate(0.2f), 3);
        Assert.True(curve.Evaluate(0.8f) > 0.5f);
    }

    [Fact]
    public void Evaluate_ClampsOutOfRangeInput()
    {
        var curve = ResponseCurve.Linear();
        Assert.Equal(0f, curve.Evaluate(-2f), 3);
        Assert.Equal(1f, curve.Evaluate(4f), 3);
    }
}
