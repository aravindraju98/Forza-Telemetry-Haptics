namespace ForzaTelemetryHaptics.Controllers;

public interface IControllerHaptics : IDisposable
{
    ControllerStatus Connect();
    void Disconnect();
    bool SetRumble(float leftMotor, float rightMotor);
    void StopRumble();
    ControllerStatus GetControllerStatus();
}
