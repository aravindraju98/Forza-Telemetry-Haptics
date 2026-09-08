# Architecture

```
Forza Horizon 6
    → UDP Data Out
        → TelemetryReceiver
            → Fh6TelemetryParser
                → TelemetryNormalizer
                    → VehicleTelemetry
                        → HapticEngine (effects + mixer + filters)
                            → IControllerHaptics
                                → XInput rumble
                                    → 8BitDo Ultimate 2C Wired
```

The WPF window only displays snapshots. Telemetry I/O and haptic mixing run on
a background loop at `HapticRateHz` (default 100 Hz).

## Projects

| Project | Role |
| --- | --- |
| `ForzaTelemetryHaptics.Core` | Telemetry, haptics, config, XInput |
| `ForzaTelemetryHaptics.App` | WPF debug / control UI |
| `ForzaTelemetryHaptics.Tests` | Parser, normalization, mixer, envelopes, disconnect |

## Threads

- UDP receive loop
- Haptic process loop
- WPF dispatcher (UI refresh ~14 Hz)

The receive loop publishes the latest normalized frame behind a lock. The
haptic loop never parses packets itself.

## Haptic mix

1. Base: engine + road (continuous presence while the car is running)
2. Dynamic: tire slip, drift, optional G-force
3. Transient: ABS pulses, traction pulses, gear kicks, impacts
4. Impact can temporarily dominate quieter layers (`ImpactPriority`)
5. Apply `GlobalGain`, then clamp to `MaximumRumble`

If telemetry goes stale, output fades to zero instead of holding the last rumble.

## Controller abstraction

`IControllerHaptics` is the only rumble boundary. The Windows implementation
calls `XInputGetState` / `XInputSetState` through P/Invoke. Tests use
`FakeController`. Other backends can be added later without touching effects.
