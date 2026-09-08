using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ForzaTelemetryHaptics.Haptics;

namespace ForzaTelemetryHaptics.App;

public partial class CurveEditor : UserControl
{
    private const int MaxPoints = 10;
    private const double Pad = 16;
    private ResponseCurve _curve = ResponseCurve.Linear();
    private int _dragIndex = -1;
    private float _inputX = -1f;

    public event EventHandler? CurveChanged;

    public CurveEditor()
    {
        InitializeComponent();
    }

    public string AxisX
    {
        get => XLabel.Text;
        set => XLabel.Text = value;
    }

    public string AxisY
    {
        get => YLabel.Text;
        set => YLabel.Text = value;
    }

    public ResponseCurve Curve
    {
        get => _curve;
        set
        {
            _curve = value ?? ResponseCurve.Linear();
            _curve.Normalize();
            Redraw();
        }
    }

    public void SetLiveInput(float x)
    {
        _inputX = x;
        Redraw();
    }

    public void ResetTo(ResponseCurve curve)
    {
        Curve = curve;
        CurveChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Plot_SizeChanged(object sender, SizeChangedEventArgs e) => Redraw();

    private void Plot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(Plot);
        var hit = HitTest(pos);
        if (hit >= 0)
        {
            _dragIndex = hit;
            Plot.CaptureMouse();
            return;
        }

        if (_curve.Points.Count >= MaxPoints)
        {
            return;
        }

        ToNorm(pos, out var x, out var y);
        if (_curve.Points.Any(p => Math.Abs(p.X - x) < 0.04f))
        {
            return;
        }

        _curve.Points.Add(new CurvePoint(x, y));
        _curve.Normalize();
        _dragIndex = Nearest(x, y);
        Plot.CaptureMouse();
        Redraw();
        CurveChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Plot_MouseMove(object sender, MouseEventArgs e)
    {
        if (_dragIndex < 0 || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        ToNorm(e.GetPosition(Plot), out var x, out var y);
        var last = _curve.Points.Count - 1;
        if (_dragIndex == 0)
        {
            x = 0f;
        }
        else if (_dragIndex == last)
        {
            x = 1f;
        }
        else
        {
            var min = _curve.Points[_dragIndex - 1].X + 0.02f;
            var max = _curve.Points[_dragIndex + 1].X - 0.02f;
            x = Math.Clamp(x, min, max);
        }

        _curve.Points[_dragIndex].X = x;
        _curve.Points[_dragIndex].Y = y;
        Redraw();
        CurveChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Plot_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _dragIndex = -1;
        Plot.ReleaseMouseCapture();
        _curve.Normalize();
        Redraw();
    }

    private void Plot_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        var hit = HitTest(e.GetPosition(Plot));
        if (hit <= 0 || hit >= _curve.Points.Count - 1)
        {
            return;
        }

        _curve.Points.RemoveAt(hit);
        _curve.Normalize();
        Redraw();
        CurveChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Redraw()
    {
        Plot.Children.Clear();
        var w = Plot.ActualWidth;
        var h = Plot.ActualHeight;
        if (w < 20 || h < 20 || _curve.Points.Count < 2)
        {
            return;
        }

        var grid = new SolidColorBrush(Color.FromRgb(44, 47, 51));
        for (var i = 1; i < 4; i++)
        {
            var gx = Pad + (w - 2 * Pad) * i / 4.0;
            var gy = Pad + (h - 2 * Pad) * i / 4.0;
            Plot.Children.Add(Line(gx, Pad, gx, h - Pad, grid, 1));
            Plot.Children.Add(Line(Pad, gy, w - Pad, gy, grid, 1));
        }

        var line = new Polyline
        {
            Stroke = new SolidColorBrush(Color.FromRgb(232, 163, 23)),
            StrokeThickness = 2,
            Fill = Brushes.Transparent
        };
        foreach (var p in _curve.Points.OrderBy(p => p.X))
        {
            line.Points.Add(FromNorm(p.X, p.Y, w, h));
        }

        Plot.Children.Add(line);

        if (_inputX >= 0f)
        {
            var y = _curve.Evaluate(_inputX);
            var live = FromNorm(_inputX, y, w, h);
            Plot.Children.Add(Line(live.X, Pad, live.X, h - Pad, new SolidColorBrush(Color.FromArgb(160, 125, 218, 90)), 1));
            Plot.Children.Add(Dot(live, 5, Color.FromRgb(125, 218, 90)));
        }

        foreach (var p in _curve.Points)
        {
            Plot.Children.Add(Dot(FromNorm(p.X, p.Y, w, h), 5, Color.FromRgb(237, 236, 232)));
        }
    }

    private int HitTest(Point pos)
    {
        var w = Plot.ActualWidth;
        var h = Plot.ActualHeight;
        for (var i = 0; i < _curve.Points.Count; i++)
        {
            var p = FromNorm(_curve.Points[i].X, _curve.Points[i].Y, w, h);
            if ((p - pos).Length <= 10)
            {
                return i;
            }
        }

        return -1;
    }

    private int Nearest(float x, float y)
    {
        var best = 0;
        var bestD = float.MaxValue;
        for (var i = 0; i < _curve.Points.Count; i++)
        {
            var dx = _curve.Points[i].X - x;
            var dy = _curve.Points[i].Y - y;
            var d = dx * dx + dy * dy;
            if (d < bestD)
            {
                bestD = d;
                best = i;
            }
        }

        return best;
    }

    private void ToNorm(Point pos, out float x, out float y)
    {
        var w = Math.Max(Plot.ActualWidth - 2 * Pad, 1);
        var h = Math.Max(Plot.ActualHeight - 2 * Pad, 1);
        x = (float)Math.Clamp((pos.X - Pad) / w, 0, 1);
        y = (float)Math.Clamp(1.0 - (pos.Y - Pad) / h, 0, 1);
    }

    private static Point FromNorm(float x, float y, double w, double h)
    {
        var pw = w - 2 * Pad;
        var ph = h - 2 * Pad;
        return new Point(Pad + x * pw, Pad + (1 - y) * ph);
    }

    private static Line Line(double x1, double y1, double x2, double y2, Brush brush, double thickness) =>
        new() { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = brush, StrokeThickness = thickness };

    private static Ellipse Dot(Point center, double r, Color color)
    {
        var dot = new Ellipse
        {
            Width = r * 2,
            Height = r * 2,
            Fill = new SolidColorBrush(color)
        };
        Canvas.SetLeft(dot, center.X - r);
        Canvas.SetTop(dot, center.Y - r);
        return dot;
    }
}
