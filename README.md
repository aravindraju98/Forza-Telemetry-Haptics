# Forza Horizon 6 Telemetry Haptics

<img src="docs/media/icon.png" width="72" alt="Telemetry Haptics icon">

Independent Windows app that turns Forza Horizon 6 **Data Out** UDP into layered XInput rumble. Built for the **8BitDo Ultimate 2C Wired**. Any XInput rumble pad works.

It is a separate process. It does not modify the game, inject DLLs, read memory, or automate play.

**Not affiliated with Microsoft, Xbox, Playground Games, Forza, or 8BitDo.**

![Mixer running next to Forza](docs/media/mixer.gif)

## Download

Get the latest Windows build from [Releases](https://github.com/aravindraju98/Forza-Telemetry-Haptics/releases). Unzip and run `ForzaTelemetryHaptics.exe`, or use `run.bat` if you have the .NET 8 SDK.

## What it does

While you drive, the pad keeps a low baseline and changes with the car:

- Engine RPM (quieter off-throttle, more texture at redline)
- Turbo boost extra on the right motor
- Road, surface, rumble strips, and puddles, panned per side
- Tire slip and wheelspin
- Drift after a start threshold
- Handbrake grab that scales with speed
- ABS and traction pulses
- Gear-change kicks
- Impacts and suspension thuds
- Optional G-force

Each layer has **Off | On**. Hover **?** for a short tip. Drag the curve like a fan graph. **LEFT MOTOR** ducks the heavier XInput motor so a centered mix does not feel left-heavy on the 2C.

A global **MAX RUMBLE** cap keeps the pad from locking at full strength.

## Requirements

- Windows 10 or 11
- Forza Horizon 6 with Data Out on (Horizon titles that send the same 324-byte packet also work)
- 8BitDo Ultimate 2C Wired, or any XInput pad with rumble, over USB
- .NET 8 SDK only if you build from source

## Forza Data Out

1. Start this app first so the UDP port is listening.
2. In Forza: **Settings → HUD and Gameplay**.
3. **Data Out** = On.
4. **Data Out IP Address** = `127.0.0.1` (same PC) or this PC’s LAN IP.
5. **Data Out IP Port** = the app port (default `5000`).
6. Drive. The header should show **LIVE** and a packet rate.

FH6 sends a **324-byte** packet while you are driving. Menus, pause, and replay send nothing; rumble fades out.

Field list: [docs/TELEMETRY.md](docs/TELEMETRY.md).

## Controller

1. Plug the 2C in over USB.
2. Use **XInput / Windows** mode.
3. 8BitDo Ultimate Software is not required.

Use **Left**, **Right**, **Both**, and **Stop** to test motors before you drive.

The left grip motor is physically stronger. **LEFT MOTOR** in the footer (default 85%) trims that so equal mix values feel closer in the hands.

Other Xbox-style XInput pads work. DInput, Switch, PlayStation, and no-rumble pads do not, unless something presents them as XInput.

## Tune

- **MIX** — live RPM, speed, gear, and per-layer bars.
- **LAYER** — select a layer, switch it off, or edit its curve and sliders.
- **?** — hover for what that control does.
- **Reset curve** — restore that layer’s default graph.
- **PROFILE** — Default, Subtle, Strong, Race, Drift, or your own name.

Gain sliders apply immediately. Bind address and port need **Apply**.

Useful `config.json` keys:

| Key | Role |
| --- | --- |
| `TelemetryBindAddress` / `TelemetryPort` | UDP listener |
| `Haptics.MaximumRumble` | Hard cap |
| `Haptics.GlobalGain` | Master scale |
| `Haptics.LeftMotorScale` | Heavier-motor trim |
| `Haptics.*Enabled` | Per-layer on/off |
| `Haptics.TelemetryTimeoutMs` | Fade after lost packets |

## Build

```powershell
dotnet restore ForzaTelemetryHaptics.sln
dotnet build ForzaTelemetryHaptics.sln -c Release
dotnet test ForzaTelemetryHaptics.sln -c Release
```

```powershell
dotnet run --project src/ForzaTelemetryHaptics.App -c Release
```

Or `run.bat`. Standalone exe:

```powershell
.\scripts\publish.ps1
```

That writes `publish\ForzaTelemetryHaptics.exe`.

## Troubleshooting

- Packet rate stays 0: Data Out is off, IP/port mismatch, or you are in a menu.
- Parser only accepts **324-byte** Horizon packets.
- Test buttons silent: Windows is not exposing rumble. This app does not talk 8BitDo firmware.
- Two pads: first XInput slot (index 0), or set `ControllerIndex` in `config.json`.

How the mixer is put together: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## License

MIT. Notices: [docs/THIRD_PARTY_NOTICES.md](docs/THIRD_PARTY_NOTICES.md) and [LICENSE](LICENSE).

No Forza assets or packet captures are included. Use official Data Out only.
