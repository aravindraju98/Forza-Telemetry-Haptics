using ForzaTelemetryHaptics.Telemetry;

namespace ForzaTelemetryHaptics.Controllers;

public sealed class XInputController : IControllerHaptics
{
    private readonly int? _preferredIndex;
    private int _index = -1;
    private bool _connected;
    private string _error = string.Empty;
    private float _lastLeft;
    private float _lastRight;

    public XInputController(int? preferredIndex = null)
    {
        _preferredIndex = preferredIndex;
    }

    public ControllerStatus Connect()
    {
        if (!OperatingSystem.IsWindows())
        {
            _connected = false;
            _error = "XInput is only available on Windows.";
            return GetControllerStatus();
        }

        RefreshConnection();
        return GetControllerStatus();
    }

    public void Disconnect()
    {
        StopRumble();
        _connected = false;
        _index = -1;
    }

    public bool SetRumble(float leftMotor, float rightMotor)
    {
        RefreshConnection();
        if (!_connected || _index < 0)
        {
            return false;
        }

        var left = (ushort)(MathUtil.Clamp01(leftMotor) * 65535f);
        var right = (ushort)(MathUtil.Clamp01(rightMotor) * 65535f);
        var vibration = new XInputNative.XinputVibration
        {
            LeftMotorSpeed = left,
            RightMotorSpeed = right
        };

        var result = XInputNative.SetState(_index, ref vibration);
        if (result == XInputNative.ErrorSuccess)
        {
            _lastLeft = MathUtil.Clamp01(leftMotor);
            _lastRight = MathUtil.Clamp01(rightMotor);
            _error = string.Empty;
            return true;
        }

        _connected = false;
        _error = "Controller disconnected while setting rumble.";
        return false;
    }

    public void StopRumble()
    {
        if (_index < 0)
        {
            return;
        }

        var vibration = new XInputNative.XinputVibration();
        try
        {
            XInputNative.SetState(_index, ref vibration);
        }
        catch
        {
            // Best-effort silence on shutdown.
        }

        _lastLeft = 0f;
        _lastRight = 0f;
    }

    public ControllerStatus GetControllerStatus()
    {
        RefreshConnection();
        return new ControllerStatus
        {
            Detected = _connected,
            ControllerType = _connected ? "XInput" : "None",
            Index = _index,
            RumbleAvailable = _connected,
            DisplayName = _connected ? "8BitDo Ultimate 2C / XInput controller" : "No XInput controller",
            Error = _connected
                ? string.Empty
                : string.IsNullOrWhiteSpace(_error)
                    ? "No XInput controller detected. Plug in the 8BitDo Ultimate 2C Wired over USB and use XInput mode."
                    : _error
        };
    }

    public (float Left, float Right) LastOutput => (_lastLeft, _lastRight);

    private void RefreshConnection()
    {
        if (!OperatingSystem.IsWindows())
        {
            _connected = false;
            return;
        }

        try
        {
            if (_connected && _index >= 0 &&
                XInputNative.GetState(_index, out _) == XInputNative.ErrorSuccess)
            {
                return;
            }

            var start = _preferredIndex ?? 0;
            var count = _preferredIndex.HasValue ? 1 : XInputNative.UserCount;
            for (var i = 0; i < count; i++)
            {
                var index = _preferredIndex ?? ((start + i) % XInputNative.UserCount);
                if (XInputNative.GetState(index, out _) == XInputNative.ErrorSuccess)
                {
                    _index = index;
                    _connected = true;
                    _error = string.Empty;
                    return;
                }
            }

            _connected = false;
            _index = -1;
            _error = "No XInput controller detected.";
        }
        catch (DllNotFoundException)
        {
            _connected = false;
            _error = "Windows XInput library was not found.";
        }
        catch (Exception ex)
        {
            _connected = false;
            _error = ex.Message;
        }
    }

    public void Dispose() => Disconnect();
}
