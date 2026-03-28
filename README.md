# vcon — Windows On-Screen Virtual Controller Overlay

**vcon** is a Windows desktop application that renders a translucent, always-on-top virtual game controller overlay. Users interact with on-screen buttons, analog sticks, triggers, and a D-pad via touch or mouse to emulate either an Xbox 360 controller (via ViGEmBus) or direct keyboard/mouse input (via Win32 `SendInput`).

The overlay sits above windowed and borderless-fullscreen games without stealing focus, bridging the gap for titles that lack native touchscreen or gamepad support.

## Who is this for?

- **Handheld PC gamers** (ROG Ally, Legion Go, Steam Deck in Windows mode, MSI Claw)
- **Accessibility users** who need on-screen input
- **Legacy game players** needing controller emulation for keyboard-only titles
- **Developers and testers** wanting a quick virtual gamepad without physical hardware

## Features

- Transparent, always-on-top overlay that does **not** steal focus from games
- **Xbox 360 controller emulation** via ViGEmBus — authentic XInput reports
- **Keyboard/mouse injection** via Win32 `SendInput` for non-controller-aware games
- Full virtual controller surface: A/B/X/Y, bumpers, triggers, analog sticks, D-pad, Start/Back/Guide
- **Multi-touch support** for simultaneous button presses
- **Customizable profiles** stored as JSON — layout, bindings, opacity, scale
- **System tray integration** with profile switching and edit mode
- **Global hotkeys** — show/hide overlay, toggle edit mode, cycle profiles
- **Layout editor** — drag and resize controls visually
- Single-instance enforcement
- Self-contained deployment (no .NET runtime install needed for end users)

## Requirements

### Build

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or later)
- GNU Make (optional, for `make` shortcuts)

### Runtime

- **Windows 10 21H2+** or **Windows 11**
- **[ViGEmBus Driver](https://github.com/nefarius/ViGEmBus/releases)** — required for Xbox 360 controller emulation mode. If not installed, vcon degrades gracefully to keyboard-only mode with a clear warning.

## Quick Start

```bash
# Clone the repository
git clone https://github.com/yourusername/vcon.git
cd vcon

# Restore, build, and run
make restore
make build
make run
```

Or without Make:

```bash
dotnet restore src/Vcon.sln
dotnet build src/Vcon.sln -c Release
dotnet run --project src/Vcon.Overlay/Vcon.Overlay.csproj
```

## Makefile Targets

| Target    | Description |
|:----------|:------------|
| `build`   | Build the solution (Release) |
| `test`    | Run all xUnit tests |
| `run`     | Launch the overlay application |
| `clean`   | Remove build artifacts |
| `lint`    | Verify formatting (`dotnet format --verify-no-changes`) |
| `fmt`     | Auto-format code |
| `restore` | Restore NuGet packages |
| `publish` | Self-contained publish for win-x64 |
| `version` | Display current version |
| `help`    | List all targets |

## Configuration

**Global settings** are stored at `%APPDATA%/vcon/settings.json`:

```json
{
  "activeProfileId": "xbox-standard",
  "hotkeys": {
    "toggleOverlay": "F9",
    "toggleEditMode": "F10",
    "cycleProfile": "F11"
  },
  "startMinimized": false,
  "startWithWindows": false,
  "logLevel": "Warning"
}
```

**Profiles** are stored as individual JSON files at `%APPDATA%/vcon/profiles/`. A default "Xbox Standard" profile is copied on first run.

### Default Hotkeys

| Hotkey | Action |
|:-------|:-------|
| F9     | Toggle overlay visibility |
| F10    | Toggle layout edit mode |
| F11    | Cycle to next profile |

## Architecture

The solution follows a clean layered architecture with strict top-down dependencies:

```
Vcon.Overlay  (WPF host — UI, services, DI composition)
      │
      ├── Vcon.Input  (ViGEm Xbox 360 emulation + SendInput keyboard/mouse injection)
      │       │
      └───────┴── Vcon.Core  (abstractions, models, serialization — zero upward deps)
```

| Project | Purpose |
|:--------|:--------|
| `Vcon.Core` | Shared interfaces (`IInputEmulator`, `IProfileManager`), models (`ControllerState`, `ControllerProfile`), JSON serialization |
| `Vcon.Input` | Xbox 360 controller emulation via ViGEmBus, keyboard/mouse injection via Win32 `SendInput`, input factory |
| `Vcon.Overlay` | WPF application host — overlay window, virtual controls, ViewModels (MVVM), system tray, global hotkeys, profile management |

See [DESIGN.md](DESIGN.md) for the full product requirements document including technical architecture, data flow, threading model, and development roadmap.

## Input Modes

### Xbox 360 Mode (XInput)

Creates a virtual Xbox 360 controller via the ViGEmBus kernel driver. Games see authentic XInput reports — full support for all buttons, analog sticks, and triggers. Requires [ViGEmBus driver](https://github.com/nefarius/ViGEmBus/releases) to be installed.

### Keyboard/Mouse Mode

Maps each virtual control to keyboard keys or mouse buttons, injected via Win32 `SendInput`. Use this for games that don't recognize XInput controllers. Analog sticks map to discrete directional keys (e.g., WASD) with a configurable threshold.

## Display Mode Support

| Mode | Supported |
|:-----|:----------|
| Windowed | Yes |
| Borderless Fullscreen | Yes |
| Fullscreen Exclusive | No — overlay is hidden by the game. Use borderless fullscreen instead. |

## Development

### Running Tests

```bash
make build
make test
```

### Project Structure

```
vcon/
├── src/
│   ├── Vcon.Core/           # Abstractions, models, config
│   ├── Vcon.Input/          # Input emulation backends
│   ├── Vcon.Overlay/        # WPF application
│   └── Vcon.sln
├── tests/
│   ├── Vcon.Core.Tests/
│   ├── Vcon.Input.Tests/
│   └── Vcon.Overlay.Tests/
├── profiles/                # Default profile templates
├── Directory.Build.props    # Centralized version
├── Directory.Packages.props # Centralized NuGet versions
└── Makefile
```

## Known Limitations

- **Fullscreen exclusive** games are not supported — the Windows DWM does not composite the overlay. Recommend borderless fullscreen.
- **UIPI**: If a game runs elevated (as Administrator) and vcon does not, `SendInput` keyboard injections are silently dropped. Either run vcon as Administrator or use Xbox emulation mode.
- **ViGEmBus retirement**: The ViGEmBus project has reduced maintenance as of late 2023. The driver remains functional and community-forked. vcon's `IInputEmulator` abstraction isolates it from the driver — a replacement can be swapped without UI or profile changes.

## License

[Apache License 2.0](LICENSE)
