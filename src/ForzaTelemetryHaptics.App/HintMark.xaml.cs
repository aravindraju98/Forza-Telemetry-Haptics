using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ForzaTelemetryHaptics.App;

public partial class HintMark : UserControl
{
    public static readonly DependencyProperty TipProperty =
        DependencyProperty.Register(nameof(Tip), typeof(string), typeof(HintMark), new PropertyMetadata(""));

    public string Tip
    {
        get => (string)GetValue(TipProperty);
        set => SetValue(TipProperty, value);
    }

    public HintMark()
    {
        InitializeComponent();
        MouseEnter += (_, _) => SetHover(true);
        MouseLeave += (_, _) => SetHover(false);
    }

    private void SetHover(bool on)
    {
        var color = (Brush)FindResource(on ? "Good" : "Muted");
        Chrome.BorderBrush = color;
        Mark.Foreground = color;
    }
}
