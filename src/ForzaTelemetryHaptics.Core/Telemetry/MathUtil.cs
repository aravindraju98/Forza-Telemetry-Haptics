namespace ForzaTelemetryHaptics.Telemetry;

public static class MathUtil
{
    public static float Clamp01(float value) => Math.Clamp(value, 0f, 1f);

    public static float SaturateDivide(float numerator, float denominator, float fallback = 0f)
    {
        if (!float.IsFinite(denominator) || MathF.Abs(denominator) < 1e-5f)
        {
            return fallback;
        }

        var result = numerator / denominator;
        return float.IsFinite(result) ? result : fallback;
    }

    public static float Max4(float a, float b, float c, float d) =>
        MathF.Max(MathF.Max(a, b), MathF.Max(c, d));

    public static float Average4(float a, float b, float c, float d) => (a + b + c + d) * 0.25f;

    public static float AbsMax(float a, float b, float c, float d) =>
        Max4(MathF.Abs(a), MathF.Abs(b), MathF.Abs(c), MathF.Abs(d));
}
