# DESIGN.md

Visual system for Forza Telemetry Haptics. Written so later UI work stays on this surface, not a generic agent dashboard.

## Intent

Operate. An instrument cluster beside the game: hairline panels, tabular numbers, one warm accent.

## Color

| Token | Hex | Use |
| --- | --- | --- |
| Canvas | `#0E0F10` | Window |
| Panel | `#16181B` | Columns |
| Line | `#2C2F33` | Rules, tracks |
| Text | `#EDECE8` | Primary |
| Muted | `#8A8880` | Labels |
| Accent | `#E8A317` | Active fill, curve |
| Good | `#7DDA5A` | Live / on |
| Warn | `#E8A317` | Caution |
| Bad | `#E24B4A` | Pad / bind errors |

No purple. No blue glow. No glass.

## Type

- UI: Segoe UI
- Numbers: Cascadia Mono, Consolas fallback
- Eyebrow: 11px, 600, letter-spaced, muted
- Value: 20px mono for RPM / speed / motors; 13px mono for secondary
- Body: 13px

## Shape

Radius 2px or none. 1px rules instead of stacked cards. 8px rhythm. Sliders and bars are thin tracks, not chunky Windows chrome.

## Layout

Header status strip. Three columns: vehicle, mixer, tune. Footer: profile + safety. No decorative hero.
