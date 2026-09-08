using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ForzaTelemetryHaptics.App;

internal static class WindowTheme
{
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaBorderColor = 34;
    private const int DwmwaCaptionColor = 35;
    private const int DwmwaTextColor = 36;

    public static void Apply(Window window)
    {
        var hwnd = new WindowInteropHelper(window).EnsureHandle();
        var dark = 1;
        DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref dark, sizeof(int));

        // COLORREF is 0x00BBGGRR. Keep caption/border on the app palette
        // so Windows does not flip the chrome to light gray when unfocused.
        var caption = 0x00100F0E; // #0E0F10
        var text = 0x00E8ECED;    // #EDECE8
        var border = 0x00332F2C;  // #2C2F33
        DwmSetWindowAttribute(hwnd, DwmwaCaptionColor, ref caption, sizeof(int));
        DwmSetWindowAttribute(hwnd, DwmwaTextColor, ref text, sizeof(int));
        DwmSetWindowAttribute(hwnd, DwmwaBorderColor, ref border, sizeof(int));
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int size);
}
