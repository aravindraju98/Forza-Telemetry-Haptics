# Forza Horizon 6 telemetry

## Interface

Forza Horizon 6 exposes a public, one-way **Data Out** UDP stream. This
application only listens for those packets. It does not modify the game, inject
code, read process memory, or send data back to FH6.

Official documentation:

https://support.forza.net/hc/en-us/articles/51744149102611-Forza-Horizon-6-Data-Out-Documentation

In-game setup: **Settings → HUD and Gameplay → Data Out**

| Setting | Recommended value |
| --- | --- |
| Data Out | On |
| IP address | `127.0.0.1` on the same PC, or this PC's LAN IP |
| Port | `5000` (must match `config.json`) |

The game sends packets at the rendering frame rate while the player is driving.
Packets are not sent in menus, pauses, replays, rewinds, or after a race ends.
The game's outbound socket commonly uses ports 5200–5299, so this app defaults
to **5000**.

## Packet

- Size: **324 bytes**
- Endianness: little-endian
- Packing: no alignment padding
- Format: fixed Horizon "Dash" layout (not selectable in FH6)

Horizon inserts three fields after `NumCylinders` and before `PositionX`
(offsets 232–243):

| Offset | Type | Field | Notes |
| --- | --- | --- | --- |
| 232 | s32 | CarGroup | Per-car category |
| 236 | f32 | SmashableVelDiff | Velocity change from smashable hits (m/s), 0 outside collisions |
| 240 | f32 | SmashableMass | Smashable mass (kg), 0 outside collisions |

FH6 does **not** include Motorsport-only `TireWear` or `TrackOrdinal`.

Wheel-on-rumble-strip fields are **int32** flags (0/1), not floats.

## Field availability

### Available (on the wire)

Engine RPM (current / idle / max), speed, velocity, acceleration (X right,
Y up, Z forward), orientation, per-wheel slip ratio / slip angle / combined
slip, wheel rotation, rumble-strip flags, puddle depth, surface rumble,
suspension travel, drivetrain type, smashable impact fields, position, power,
torque, tire temps, boost, fuel, lap/race info, pedal and steer inputs, gear.

### Unavailable (not fabricated)

There is no official ABS-active flag, traction-control flag, or generic
collision enum. Those are **not** invented as Available fields.

### Derived

| Value | How it is derived | Why this is defensible |
| --- | --- | --- |
| Normalized RPM | `(rpm - idle) / (max - idle)` | Idle and redline are on the wire |
| Normalized speed | speed / configurable max | Speed is on the wire |
| Pedal 0–1 | byte / 255 | Inputs are 0–255 |
| Aggregate / corner slip | `abs(combined slip)` clamped to 0–1 | Per-wheel combined slip is on the wire |
| ABS estimate | high brake + locking slip (negative slip ratio) above a speed floor | FH6 has no ABS flag; this matches lock/unlock under hard braking |
| Traction estimate | throttle + positive slip on driven wheels | Driven wheels come from `DrivetrainType` (FWD/RWD/AWD) |
| Smashable impact | `SmashableVelDiff` and `SmashableMass` | Official FH6 fields, zero outside collisions |
| Boost 0–1 | raw `Boost` if it is already 0–1, otherwise `/ 18` | Official field; cars report PSI or a 0–1 flag |
| Puddle left/right | max of that side's `WheelInPuddleDepth` | Official per-wheel depths |
| Suspension thud | sudden compression of `SuspensionTravelMeters` | Pothole/jump is not a dedicated flag |
| Landing impact | large vertical acceleration delta | Jump/landing is not a dedicated flag |
| Speed impact | sudden speed drop that is too sharp for normal braking | Wall/car hits often do not set smashable fields |
| Accel impact | large change in acceleration magnitude | Extra cue when smashable fields stay at 0 |
| Gear-shift hint | sudden RPM drop while throttle is applied and the gear byte did not move | Backup if the gear field is sticky |

If a later FH6 patch publishes official ABS/TCS bits, the parser can be swapped
without changing the haptic engine: `ITelemetryParser` is isolated.
