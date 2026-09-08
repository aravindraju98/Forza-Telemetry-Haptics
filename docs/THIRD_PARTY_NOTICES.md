# Third-party notices

This project is original software licensed under the MIT License. It does not
copy implementation code from other telemetry or haptic projects.

## Runtime and libraries

| Dependency | Used for | License |
| --- | --- | --- |
| .NET 8 (Microsoft) | Runtime, BCL, WPF | MIT |
| Windows XInput (`xinput1_4.dll` / `xinput1_3.dll`) | Controller rumble | Windows component (not redistributed) |
| xunit | Automated tests | Apache License 2.0 |
| xunit.runner.visualstudio | Test runner | Apache License 2.0 / MIT |
| Microsoft.NET.Test.Sdk | Test SDK | MIT |

No SharpDX, no 8BitDo SDK, and no Forza game files are included.

## Telemetry specification

Packet field names, types, and the 324-byte Horizon Data Out layout are taken
from publicly documented Forza Horizon 6 Data Out documentation:

https://support.forza.net/hc/en-us/articles/51744149102611-Forza-Horizon-6-Data-Out-Documentation

That specification is Microsoft / Playground Games documentation. This project
does not claim ownership of Forza, Xbox, or related trademarks.

## Trademarks

Forza, Forza Horizon, Xbox, and Playground Games are trademarks of their
respective owners. 8BitDo is a trademark of its owner. This project is an
independent third-party application and is not affiliated with or endorsed by
Microsoft, Xbox, Playground Games, Forza, or 8BitDo.
