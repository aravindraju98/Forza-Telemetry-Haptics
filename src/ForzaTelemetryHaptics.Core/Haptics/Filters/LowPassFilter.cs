namespace ForzaTelemetryHaptics.Haptics.Filters;

public sealed class LowPassFilter
{
    private float _current;

    public float Update(float target, float smoothingSeconds, float deltaSeconds)
    {
        var tau = Math.Max(smoothingSeconds, 0.001f);
        var alpha = 1f - MathF.Exp(-deltaSeconds / tau);
        _current += (target - _current) * alpha;
        return _current;
    }

    public void Reset(float value = 0f) => _current = value;
}
