# Privacy Policy

Last updated: 2026-11-12

Telemetry Haptics is a free, open-source Windows app. It does not collect, store, or sell personal data, and it has no accounts, analytics, advertising, or crash reporting.

## What the app does with data

- **Forza telemetry.** The app listens on a local UDP port for the "Data Out" packets Forza sends. They are processed in memory to drive your controller's rumble and are never saved or sent anywhere.
- **Controller output.** Rumble is sent to your local XInput controller only.
- **Settings.** Your settings and tune profiles are saved as `config.json` and a `profiles` folder next to the exe, on your own PC. Nothing is uploaded.

## Network use

The only internet connection the app makes is the optional **Auto update** check, which is **off by default**. When you turn it on, the app:

1. Requests the latest release info from the GitHub API (`api.github.com`), and
2. If you choose to install an update, downloads the release zip from GitHub.

These requests go directly to GitHub and are subject to [GitHub's Privacy Statement](https://docs.github.com/en/site-policy/privacy-policies/github-general-privacy-statement). GitHub may see your IP address and a `ForzaTelemetryHaptics` user agent. The app sends no other identifiers and the developer receives no data.

If Auto update is off, the app makes no internet connections at all.

## Third parties

No third-party SDKs, trackers, or services are used. Libraries are listed in [docs/THIRD_PARTY_NOTICES.md](docs/THIRD_PARTY_NOTICES.md).

## Children

The app collects no personal information from anyone, including children.

## Changes

If this policy changes, the update will appear in this file with a new date.

## Contact

Questions: open an issue at https://github.com/aravindraju98/Forza-Telemetry-Haptics/issues
