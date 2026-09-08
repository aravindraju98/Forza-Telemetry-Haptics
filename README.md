# Forza Horizon 6 Telemetry Haptics

Independent Windows desktop app that turns Forza Horizon 6 **Data Out** UDP
telemetry into continuous, layered XInput rumble for an **8BitDo Ultimate 2C
Wired** controller.

This is a separate process. It does not modify Forza Horizon 6, inject DLLs,
read game memory, bypass anti-cheat, or automate gameplay.

**Not affiliated with or endorsed by Microsoft, Xbox, Playground Games, Forza, or 8BitDo.**

## What it does

While you drive, the controller keeps a subtle haptic presence and changes with
vehicle state:

- Engine RPM (nonlinear curve + faster modulation near redline), quieter off-throttle
- Turbo boost extra
- Road / surface rumble and rumble strips, panned per side
- Puddle drag from per-wheel puddle depth
- Tire slip and wheelspin (directional when per-wheel data exists)
- Drift intensity from rear slip / slip angle
- Handbrake grab
- ABS-like pulsing (derived from brake + locking slip)
- Traction-control-like pulsing (derived from throttle + driven-wheel spin)
- Gear-change kicks
- Smashable collisions, landings, and suspension thuds
- Optional G-force bias

Multiple effects are mixed, smoothed, and clamped by a global safety limit so
the pad does not lock at full rumble.

## Requirements

- Windows 10 or 11
- .NET 8 SDK to build
- Forza Horizon 6 with Data Out enabled (for live telemetry)
- 8BitDo Ultimate 2C Wired (or any XInput pad with rumble) over USB

## How to build

Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0), then:

```powershell
dotnet restore ForzaTelemetryHaptics.sln
dotnet build ForzaTelemetryHaptics.sln -c Release
dotnet test ForzaTelemetryHaptics.sln -c Release
```

If `dotnet` is not on PATH, a user-local SDK install works the same way:

```powershell
& "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe" test ForzaTelemetryHaptics.sln -c Release
```

## How to run

```powershell
dotnet run --project src/ForzaTelemetryHaptics.App -c Release
```

Or publish a standalone exe:

```powershell
dotnet publish src/ForzaTelemetryHaptics.App/ForzaTelemetryHaptics.App.csproj `
  -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true `
  -o publish
```

Then run `publish\ForzaTelemetryHaptics.exe`.

## How to configure FH6 telemetry

1. Start this app first so the UDP port is listening.
2. In Forza Horizon 6 open **Settings → HUD and Gameplay**.
3. Set **Data Out** to **On**.
4. Set **Data Out IP Address** to `127.0.0.1` (same PC) or this PC's LAN IP.
5. Set **Data Out IP Port** to the app port (default `5000`).
6. Drive. The UI should show **Connected: YES** and a rising packet rate.

FH6 sends a fixed **324-byte** packet while you are actively driving. Menus,
pauses, and replays produce no packets; the app then fades rumble out.

Details and field availability: [docs/TELEMETRY.md](docs/TELEMETRY.md).

## How to configure the controller

1. Plug the 8BitDo Ultimate 2C Wired in over USB.
2. Use the controller's normal **XInput / Windows** mode (Xbox-style).
3. Do not use a DInput-only mode if rumble is missing.
4. 8BitDo Ultimate Software is **not** required.

The UI should report:

```
Controller detected: YES
Controller type: XInput
Controller index: 0
Rumble: AVAILABLE
```

Use **Test left motor**, **Test right motor**, and **Test both motors** to
confirm rumble before driving.

## How to enable XInput mode

On the Ultimate 2C Wired, Windows XInput is the default USB mode. If Windows
shows a generic DirectInput device instead:

- Unplug / replug the USB cable
- Confirm the switch/mode is set for Windows / XInput (see the 8BitDo manual)
- Close overlays that exclusive-grab the pad if detection fails
- Try another USB port (avoid some unpowered hubs)

This app only uses the standard Windows XInput API (`XInputGetState` /
`XInputSetState`). It does not reverse engineer 8BitDo firmware.

## Other XInput controllers

Any Windows **XInput** pad with rumble will work. The 8BitDo Ultimate 2C Wired
is the design target, but the app does not use 8BitDo-specific firmware.

These are fine if Windows sees them as an Xbox-style controller:

- Xbox wired / wireless
- Other 8BitDo pads in **XInput** mode
- Third-party Xbox-layout pads that expose rumble through XInput

These will not work well:

- DInput-only / Android / Switch / PlayStation modes
- Pads with no rumble motors
- DualSense / DualShock unless Steam Input or a driver presents them as XInput

If two pads are plugged in, the app uses the first XInput slot (usually index
0). Force a slot with `ControllerIndex` in `config.json`. Use **Test both
motors** to confirm rumble on that pad.

## How to configure haptics

Use the **Curves** tab like a fan curve: pick an effect, then drag points on
the graph (click to add, right-click to delete). X is the input (RPM, speed,
slip, and so on) and Y is rumble strength. A green marker shows the live
value. Sliders under the graph still set overall gain. Profiles store the
points. **Maximum rumble** and **Global gain** stay in the Safety panel.

Important keys:

| Key | Role |
| --- | --- |
| `TelemetryBindAddress` / `TelemetryPort` | UDP listener |
| `Haptics.EngineBase` / `EngineGain` / `EngineExponent` | Idle-to-redline curve |
| `Haptics.EngineModulation` / `EngineModulationHz` | Engine texture |
| `Haptics.EngineThrottleBlend` | How much off-throttle quiets the engine |
| `Haptics.BoostGain` | Turbo extra |
| `Haptics.RoadGain` / `RoadPuddleGain` | Speed / surface / puddle layer |
| `Haptics.HandbrakeGain` | E-brake grab |
| `Haptics.SlipGain` / `DriftGain` | Tire dynamics |
| `Haptics.AbsGain` / `AbsFrequencyHz` / `AbsPulseWidth` | ABS pulses |
| `Haptics.TractionGain` / `TractionFrequencyHz` | Traction pulses |
| `Haptics.GearShiftGain` / `GearShiftDurationMs` | Shift kick |
| `Haptics.ImpactGain` | Collisions / landings |
| `Haptics.GForceGain` / `GForceEnabled` | Optional G layer |
| `Haptics.GlobalGain` | Master scale |
| `Haptics.MaximumRumble` | Hard ceiling (default 0.35) |
| `Haptics.TelemetryTimeoutMs` | Fade-out after lost packets |

Restart is not required for gain sliders. Bind address / port need **Apply / restart**.

## How to create profiles

Built-in profiles: **Default**, **Subtle**, **Strong**, **Race**, **Drift**.

In the UI: choose a profile and click **Load**, or type a name and **Save as**.

Profiles are JSON files in the `profiles` folder. Saving writes the current
haptic settings so you can switch setups without recompiling.

## Troubleshooting controller detection

- Confirm the pad is XInput (it should appear as an Xbox controller).
- Test rumble with the built-in buttons. If those are silent, the OS is not
  exposing rumble — this app cannot invent a private 8BitDo protocol.
- Only the first connected XInput user (0–3) is used unless `ControllerIndex`
  is set in `config.json`.
- After unplugging, the app stops sending rumble until the pad reconnects.

## Troubleshooting telemetry

- Packet rate stays 0: Data Out is off, IP/port mismatch, or you are in a menu.
- Malformed packets: another app may be sending non-FH6 UDP to the same port,
  or a future patch changed the size. This parser accepts **324 bytes only**.
- RPM looks right but speed/gear are nonsense: the packet is not the Horizon
  324-byte layout. FH6 should not offer Motorsport sled/dash format options.
- Rumble dies in menus: expected. Telemetry stops, then the app fades out.

## How the haptic engine works

Each effect returns a `HapticSignal` (`left_motor`, `right_motor` in 0–1).
Continuous effects (engine, road, slip) are low-pass smoothed. Transients
(ABS, traction, gear, impact) stay sharp. The mixer layers them, lets a
collision temporarily dominate, then clamps to `MaximumRumble`.

Architecture: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Dependencies and licenses

See [docs/THIRD_PARTY_NOTICES.md](docs/THIRD_PARTY_NOTICES.md) and [LICENSE](LICENSE).

| Dependency | License |
| --- | --- |
| This project | MIT |
| .NET 8 / WPF | MIT |
| Windows XInput | Windows component, not redistributed |
| xunit / test SDK | Apache 2.0 / MIT |

No Forza assets, executables, or proprietary packet captures are included.

## Legal

Use only the official Data Out interface. Do not patch the game, inject into
the process, or use this project as a cheat or gameplay advantage.
