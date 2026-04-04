# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **`make install-vigembus`**: Makefile target that downloads the latest ViGEmBus x64 MSI from GitHub releases and launches the installer with UAC elevation.
- **Tray menu "Install ViGEmBus Driver..."**: When the driver is missing, the tray context menu shows an option that downloads the latest installer from GitHub and runs it with UAC elevation. Shows balloon-tip progress at each step (checking release, downloading, launching installer) with specific error messages for network failures, timeouts, UAC cancellation, and installer errors. After successful installation, vcon automatically reconnects the virtual controller. All steps are also logged for diagnostics.
- **Tray menu "Xbox Controller Mode" / "Keyboard Mode"**: Two radio-style menu items to switch the active profile's input mode on the fly. Changes the mode, saves the profile, and reconnects the emulator. Check marks indicate the currently active mode.
- **Tray menu "Game Controllers..."**: Opens the Windows Game Controllers control panel (`joy.cpl`) directly from the tray menu.

### Fixed

- **Emulator connection feedback**: Surface `EmulatorConnectionStatus` on `OverlayViewModel`. On startup, show a MessageBox dialog when the virtual controller fails to connect (ViGEmBus driver missing or connection error) with instructions to use the tray menu installer. On subsequent profile switches, show a tray balloon notification. Previously failures were silently logged to file with no user-visible indication.
- **ViGEmBus installer asset matching**: Match the current release naming convention (`ViGEmBus*.exe`) instead of the legacy `*Setup_x64.msi` pattern that no longer exists in recent releases.
- **Emulator disposal on exit**: `OnExit` now calls `OverlayViewModel.DisconnectEmulator()` instead of resolving `IInputEmulator` from DI (which was never registered, so the emulator was leaked on shutdown).

## [0.1.0] - 2026-03-27

### Fixed

- **Empty overlay on first run**: Copy `profiles/*.json` into the overlay build output so `ProfileManager` can seed `%APPDATA%/vcon/profiles/` from `AppContext.BaseDirectory/profiles`. Without that folder beside `vcon.exe`, the active profile was missing and the app fell back to an empty layout (tray only, no on-screen controls).
- **Show/Hide overlay**: Keep `OverlayWindow.Visibility` in sync with `OverlayViewModel.IsVisible` (handler on `PropertyChanged`) so tray and hotkey toggle show or hide the window — avoid `{StaticResource}` on the root `Window` (loads before `Window.Resources`) and WPF/WinForms `Binding` name clashes.

### Added

- **Solution structure**: `Vcon.Core`, `Vcon.Input`, `Vcon.Overlay` projects with `Vcon.sln`
- **Vcon.Core**: `IInputEmulator`, `IProfileManager`, `IControllerStateObserver` abstractions; `ControllerState`, `ControllerProfile`, `ControlDefinition`, `InputBinding`, `AppSettings` models; `ProfileSerializer` for JSON round-trip
- **Vcon.Input**: `ViGEmEmulator` for Xbox 360 controller emulation via ViGEmBus; `SendInputEmulator` for Win32 keyboard/mouse injection; `NativeMethods` P/Invoke declarations; `InputEmulatorFactory`; `ViGEmAvailability` runtime driver detection
- **Vcon.Overlay**: WPF overlay window (transparent, topmost, no-activate, click-through); `VirtualButton`, `VirtualStick`, `VirtualTrigger`, `VirtualDPad` custom controls with multi-touch support; `OverlayViewModel` with dictionary-based input dispatch; `EditorViewModel` for layout editing; `ProfileManager` with atomic JSON persistence; `GlobalHotkeyService` (F9/F10/F11 defaults); `TrayIconService` with context menu
- **App lifecycle**: Single-instance via named mutex; DI composition via `Microsoft.Extensions.DependencyInjection`; Serilog file logging; unhandled exception handler
- **Default profile**: "Xbox Standard" profile (`profiles/xbox-standard.json`) with full controller layout, Xbox-inspired ABXY colors, and keyboard fallback bindings
- **Build infrastructure**: `Directory.Build.props` (centralized versioning), `Directory.Packages.props` (centralized NuGet versions), `Makefile` with `build`, `test`, `run`, `clean`, `lint`, `fmt`, `restore`, `publish`, `version`, `help` targets
- **Tests**: xUnit test projects for Core (state, serialization, bindings), Input (key parsing, emulator API), and Overlay (ViewModel logic)
- **Documentation**: `DESIGN.md` (full PRD), `README.md` (features, setup, architecture), `CHANGELOG.md`, `AGENTS.md` (coding conventions)
