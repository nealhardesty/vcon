# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-03-27

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
