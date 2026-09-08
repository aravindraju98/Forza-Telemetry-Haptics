namespace ForzaTelemetryHaptics.Haptics.Filters;

public sealed class PulseGenerator
{
    private float _phase;

    public float Update(bool active, float frequencyHz, float pulseWidth, float deltaSeconds)
    {
        if (!active || frequencyHz <= 0f)
        {
            _phase = 0f;
            return 0f;
        }

        _phase += frequencyHz * deltaSeconds;
        if (_phase >= 1f)
        {
            _phase -= MathF.Floor(_phase);
        }

        return _phase < Math.Clamp(pulseWidth, 0.05f, 0.95f) ? 1f : 0f;
    }

    public void Reset() => _phase = 0f;
}
