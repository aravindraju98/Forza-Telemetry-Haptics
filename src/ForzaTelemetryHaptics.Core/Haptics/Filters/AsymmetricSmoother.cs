namespace ForzaTelemetryHaptics.Haptics.Filters;

public sealed class AsymmetricSmoother
{
    private float _current;

    public float Current => _current;

    public float Update(float target, float attackSeconds, float releaseSeconds, float deltaSeconds)
    {
        var tau = target > _current ? Math.Max(attackSeconds, 0.001f) : Math.Max(releaseSeconds, 0.001f);
        var alpha = 1f - MathF.Exp(-deltaSeconds / tau);
        _current += (target - _current) * alpha;
        return _current;
    }

    public void Reset(float value = 0f) => _current = value;
}
