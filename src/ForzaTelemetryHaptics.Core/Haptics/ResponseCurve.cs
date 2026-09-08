using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Haptics;

public sealed class CurvePoint
{
    public float X { get; set; }
    public float Y { get; set; }

    public CurvePoint()
    {
    }

    public CurvePoint(float x, float y)
    {
        X = x;
        Y = y;
    }
}

/// <summary>
/// Fan-style response curve: input 0–1 to rumble 0–1 through editable points.
/// </summary>
public sealed class ResponseCurve
{
    public List<CurvePoint> Points { get; set; } = LinearPoints();

    public static ResponseCurve Linear() => new() { Points = LinearPoints() };

    public static ResponseCurve Power(float exponent)
    {
        var exp = Math.Max(exponent, 0.2f);
        return new ResponseCurve
        {
            Points =
            [
                new(0f, 0f),
                new(0.25f, MathF.Pow(0.25f, exp)),
                new(0.5f, MathF.Pow(0.5f, exp)),
                new(0.75f, MathF.Pow(0.75f, exp)),
                new(1f, 1f)
            ]
        };
    }

    public static ResponseCurve Delayed(float start)
    {
        var x = Math.Clamp(start, 0.02f, 0.9f);
        return new ResponseCurve
        {
            Points =
            [
                new(0f, 0f),
                new(x, 0f),
                new(1f, 1f)
            ]
        };
    }

    public static ResponseCurve Ensure(ResponseCurve? curve, ResponseCurve fallback)
    {
        if (curve?.Points is { Count: >= 2 })
        {
            curve.Normalize();
            return curve;
        }

        return fallback.Clone();
    }

    public float Evaluate(float input)
    {
        var x = MathUtil.Clamp01(input);
        var pts = Points;
        if (pts is null || pts.Count == 0)
        {
            return x;
        }

        if (pts.Count == 1)
        {
            return MathUtil.Clamp01(pts[0].Y);
        }

        var ordered = pts.OrderBy(p => p.X).ToArray();
        if (x <= ordered[0].X)
        {
            return MathUtil.Clamp01(ordered[0].Y);
        }

        for (var i = 1; i < ordered.Length; i++)
        {
            if (x > ordered[i].X)
            {
                continue;
            }

            var a = ordered[i - 1];
            var b = ordered[i];
            var span = b.X - a.X;
            var t = span < 1e-5f ? 1f : (x - a.X) / span;
            return MathUtil.Clamp01(a.Y + (b.Y - a.Y) * t);
        }

        return MathUtil.Clamp01(ordered[^1].Y);
    }

    public void Normalize()
    {
        var pts = Points ?? [];
        if (pts.Count == 0)
        {
            Points = LinearPoints();
            return;
        }

        foreach (var p in pts)
        {
            p.X = MathUtil.Clamp01(p.X);
            p.Y = MathUtil.Clamp01(p.Y);
        }

        Points = pts.OrderBy(p => p.X).ToList();
        if (Points[0].X > 0.001f)
        {
            Points.Insert(0, new CurvePoint(0f, Points[0].Y));
        }

        if (Points[^1].X < 0.999f)
        {
            Points.Add(new CurvePoint(1f, Points[^1].Y));
        }

        Points[0].X = 0f;
        Points[^1].X = 1f;
    }

    public ResponseCurve Clone()
    {
        return new ResponseCurve
        {
            Points = (Points ?? []).Select(p => new CurvePoint(p.X, p.Y)).ToList()
        };
    }

    private static List<CurvePoint> LinearPoints() => [new(0f, 0f), new(1f, 1f)];
}
