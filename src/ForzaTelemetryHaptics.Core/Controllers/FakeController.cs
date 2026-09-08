namespace ForzaTelemetryHaptics.Controllers;

/// <summary>In-memory controller used by tests and headless runs.</summary>
public sealed class FakeController : IControllerHaptics
{
    public float LastLeft { get; private set; }
    public float LastRight { get; private set; }
    public int SetRumbleCount { get; private set; }
    public bool Connected { get; set; } = true;
    public int Index { get; set; }

    public ControllerStatus Connect() => GetControllerStatus();

    public void Disconnect()
    {
        Connected = false;
        StopRumble();
    }

    public bool SetRumble(float leftMotor, float rightMotor)
    {
        if (!Connected)
        {
            return false;
        }

        LastLeft = leftMotor;
        LastRight = rightMotor;
        SetRumbleCount++;
        return true;
    }

    public void StopRumble()
    {
        LastLeft = 0f;
        LastRight = 0f;
    }

    public ControllerStatus GetControllerStatus() => new()
    {
        Detected = Connected,
        ControllerType = Connected ? "Fake" : "None",
        Index = Connected ? Index : -1,
        RumbleAvailable = Connected,
        DisplayName = Connected ? "Fake controller" : "Disconnected",
        Error = Connected ? string.Empty : "Fake controller disconnected."
    };

    public void Dispose() => Disconnect();
}
