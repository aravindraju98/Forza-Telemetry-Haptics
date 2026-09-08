using System.Runtime.InteropServices;

namespace ForzaTelemetryHaptics.Controllers;

/// <summary>
/// Direct P/Invoke to the Windows XInput API. No third-party wrapper.
/// xinput1_4.dll ships with Windows 8+; xinput1_3.dll is the fallback.
/// </summary>
internal static class XInputNative
{
    public const int UserCount = 4;
    public const uint ErrorSuccess = 0;
    public const uint ErrorDeviceNotConnected = 1167;

    [StructLayout(LayoutKind.Sequential)]
    public struct XinputVibration
    {
        public ushort LeftMotorSpeed;
        public ushort RightMotorSpeed;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XinputGamepad
    {
        public ushort Buttons;
        public byte LeftTrigger;
        public byte RightTrigger;
        public short ThumbLX;
        public short ThumbLY;
        public short ThumbRX;
        public short ThumbRY;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XinputState
    {
        public uint PacketNumber;
        public XinputGamepad Gamepad;
    }

    private static readonly bool UseLegacy;

    static XInputNative()
    {
        UseLegacy = !IsLibraryAvailable("xinput1_4.dll");
    }

    public static uint GetState(int userIndex, out XinputState state)
    {
        return UseLegacy ? LegacyGetState(userIndex, out state) : ModernGetState(userIndex, out state);
    }

    public static uint SetState(int userIndex, ref XinputVibration vibration)
    {
        return UseLegacy ? LegacySetState(userIndex, ref vibration) : ModernSetState(userIndex, ref vibration);
    }

    [DllImport("xinput1_4.dll", EntryPoint = "XInputGetState")]
    private static extern uint ModernGetState(int dwUserIndex, out XinputState pState);

    [DllImport("xinput1_4.dll", EntryPoint = "XInputSetState")]
    private static extern uint ModernSetState(int dwUserIndex, ref XinputVibration pVibration);

    [DllImport("xinput1_3.dll", EntryPoint = "XInputGetState")]
    private static extern uint LegacyGetState(int dwUserIndex, out XinputState pState);

    [DllImport("xinput1_3.dll", EntryPoint = "XInputSetState")]
    private static extern uint LegacySetState(int dwUserIndex, ref XinputVibration pVibration);

    private static bool IsLibraryAvailable(string name)
    {
        try
        {
            return NativeLibrary.TryLoad(name, out _);
        }
        catch
        {
            return false;
        }
    }
}
