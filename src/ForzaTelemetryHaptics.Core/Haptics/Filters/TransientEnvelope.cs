namespace ForzaTelemetryHaptics.Haptics.Filters;

public sealed class TransientEnvelope
{
    private float _level;
    private float _hold;

    public float Level => _level;

    public void Trigger(float intensity, float durationSeconds)
    {
        _level = Math.Max(_level, Math.Clamp(intensity, 0f, 1f));
        _hold = Math.Max(_hold, durationSeconds);
    }

    public float Update(float attackSeconds, float decaySeconds, float deltaSeconds)
    {
        _ = attackSeconds;
        if (_hold > 0f)
        {
            _hold -= deltaSeconds;
            return _level;
        }

        var decay = Math.Max(decaySeconds, 0.001f);
        _level = Math.Max(0f, _level - deltaSeconds / decay);
        return _level;
    }

    public void Reset()
    {
        _level = 0f;
        _hold = 0f;
    }
}
