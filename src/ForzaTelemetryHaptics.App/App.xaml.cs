using System.Windows;
using System.Windows.Threading;

namespace ForzaTelemetryHaptics.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnUnhandledException;
        base.OnStartup(e);
    }

    private static void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            foreach (Window window in Current.Windows)
            {
                if (window is MainWindow main)
                {
                    main.ForceSilence();
                }
            }
        }
        catch
        {
            // last-resort silence only
        }
    }
}
