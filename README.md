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
- **Anchor-based positioning** — controls can be anchored to any screen edge (left/right, top/bottom) for resolution-independent layouts
- **Taskbar-aware layout** — bottom-anchored controls automatically offset above the Windows taskbar
- **Flexible button shapes** — circular, rounded-rectangle, or pill shapes via configurable corner radius
- **System tray integration** with profile switching, input mode selection, and edit mode
- **Global hotkeys** — show/hide overlay (F9), toggle edit mode (F10), cycle profiles (F11)
- **Layout editor** — drag controls to reposition, resize via handles, with anchor-aware coordinate conversion
- **Profile snapshot/restore** — save or discard layout edits explicitly via the tray menu
- **Locked default profile** — the built-in "Xbox Standard" profile is protected from in-app editing; attempting to edit it auto-clones to a new profile
- **Profile cloning** — duplicate any profile from the tray menu
- **ViGEmBus driver installer** — one-click download and install from the tray menu
- Single-instance enforcement
- Self-contained deployment (no .NET runtime install needed for end users)

## Requirements

### Build

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or later)
- GNU Make (optional, for `make` shortcuts)

### Runtime

- **Windows 10 21H2+** or **Windows 11**
- **[ViGEmBus Driver](https://github.com/nefarius/ViGEmBus/releases)** — required for Xbox 360 controller emulation mode. If not installed, vcon degrades gracefully to keyboard-only mode with a clear warning. You can install the driver directly from vcon's tray menu.

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

| Target             | Description |
|:-------------------|:------------|
| `build`            | Build the solution (Release) |
| `test`             | Run all xUnit tests |
| `run`              | Launch the overlay application |
| `clean`            | Remove build artifacts |
| `clean-profiles`   | Remove user profiles from `%APPDATA%/vcon/profiles` (forces re-copy of defaults on next run) |
| `lint`             | Verify formatting (`dotnet format --verify-no-changes`) |
| `fmt`              | Auto-format code |
| `restore`          | Restore NuGet packages |
| `publish`          | Self-contained publish for win-x64 |
| `install-vigembus` | Download and install the ViGEmBus driver (requires admin) |
| `version`          | Display current version |
| `help`             | List all targets |

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

**Profiles** are stored as individual JSON files at `%APPDATA%/vcon/profiles/`. A default "Xbox Standard" profile is copied on first run. You can open this directory from the tray menu via **Profiles > Open Profiles Directory**.

### Default Hotkeys

| Hotkey | Action |
|:-------|:-------|
| F9     | Toggle overlay visibility |
| F10    | Toggle layout edit mode |
| F11    | Cycle to next profile |

## Using Edit Mode

Edit mode lets you visually reposition and resize controls directly on the overlay.

### Entering Edit Mode

- Press **F10**, or right-click the tray icon and select **Edit Mode**.
- If the active profile is the locked default ("Xbox Standard"), vcon automatically clones it to "Xbox Standard (copy)" and enters edit mode on the clone.

### Editing Controls

- **Move**: Click and drag any control to reposition it. The overlay shows a cyan border on each control to indicate it is editable.
- **Resize**: Each control has a small cyan square at the bottom-right corner. Drag it to change the control's size.
- While in edit mode, controller input emulation is paused — your edits won't send unintended inputs to games.

### Saving or Discarding

- Right-click the tray icon, open the **Profiles** submenu.
- **Save Profile**: Persists all position and size changes to disk.
- **Discard Changes**: Reverts the profile to its state before editing began.
- The Save/Discard options only appear while edit mode is active.

### Profile Management

From the **Profiles** submenu you can also:

- **Clone Active Profile...**: Creates a copy of the current profile and switches to it.
- **Open Profiles Directory**: Opens the `%APPDATA%/vcon/profiles/` folder in Windows Explorer for manual editing.

## System Tray Menu

Right-click the vcon tray icon to access:

| Menu Item | Description |
|:----------|:------------|
| Show/Hide Overlay | Toggle overlay visibility |
| Edit Mode | Enter layout editing mode |
| Xbox Controller Mode | Switch active profile to XInput emulation (checked when active) |
| Keyboard Mode | Switch active profile to keyboard/mouse injection (checked when active) |
| Profiles > | Submenu: profile list, save/discard (edit mode), clone, open directory |
| Install ViGEmBus Driver... | One-click driver download and install (shown only when driver is missing) |
| Game Controllers... | Open the Windows Game Controllers panel (`joy.cpl`) |
| Exit | Shut down vcon |

## Profile JSON Format

Profiles support anchor-based positioning for resolution-independent layouts. Each control's position is measured as a normalized offset (0.0–1.0) from a configurable screen edge.

```json
{
  "id": "xbox-standard",
  "name": "Xbox Standard",
  "mode": "xInput",
  "opacity": 0.7,
  "scale": 1.0,
  "controls": [
    {
      "id": "a-button",
      "type": "button",
      "label": "a",
      "position": {
        "x": 0.045,
        "y": 0.115,
        "hAnchor": "right",
        "vAnchor": "bottom"
      },
      "size": { "width": 56, "height": 56 },
      "style": { "fill": "#107C10", "stroke": "#0E6B0E" },
      "binding": { "xInput": "A", "keyboard": "Space" }
    },
    {
      "id": "left-bumper",
      "type": "button",
      "label": "lb",
      "position": {
        "x": 0.05,
        "y": 0.19,
        "hAnchor": "left",
        "vAnchor": "bottom"
      },
      "size": { "width": 50, "height": 65 },
      "style": { "fill": "#3A3A3A", "stroke": "#555555", "cornerRadius": 10 },
      "binding": { "xInput": "LeftShoulder", "keyboard": "Q" }
    }
  ]
}
```

### Position Anchoring

| Property | Values | Description |
|:---------|:-------|:------------|
| `hAnchor` | `left` (default), `right` | Which horizontal edge the X offset is measured from |
| `vAnchor` | `top` (default), `bottom` | Which vertical edge the Y offset is measured from |

When `vAnchor` is `bottom`, the Y offset is measured upward from the bottom of the usable screen area (above the taskbar). When `hAnchor` is `right`, the X offset is measured leftward from the right screen edge.

### Style Options

| Property | Description |
|:---------|:------------|
| `fill` | Background color (hex, e.g. `"#107C10"`) |
| `stroke` | Border color (hex) |
| `cornerRadius` | Corner radius in reference pixels. Omit for circular buttons (auto-computed from size). Set to a small value like `10` for rounded rectangles. |

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
| `Vcon.Core` | Shared interfaces (`IInputEmulator`, `IProfileManager`), models (`ControllerState`, `ControllerProfile`, `PositionInfo` with anchors), JSON serialization |
| `Vcon.Input` | Xbox 360 controller emulation via ViGEmBus, keyboard/mouse injection via Win32 `SendInput`, input factory |
| `Vcon.Overlay` | WPF application host — overlay window, virtual controls, ViewModels (MVVM), system tray, global hotkeys, profile management, layout editor |

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
