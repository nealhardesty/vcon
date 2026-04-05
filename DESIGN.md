# PRD: vcon — Windows On-Screen Virtual Controller Overlay

## 1. Executive Summary

**vcon** is a Windows desktop application that renders a translucent, always-on-top virtual game controller overlay. Users interact with on-screen buttons, analog sticks, triggers, and a D-pad via touch or mouse to emulate either an Xbox 360 controller (via ViGEmBus) or direct keyboard/mouse input (via Win32 `SendInput`). The overlay sits above windowed and borderless-fullscreen games without stealing focus, bridging the gap for titles that lack native touchscreen or gamepad support.

The app targets handheld PC gamers, accessibility users, and anyone needing a software-based controller for legacy or non-gamepad-aware titles.

---

## 2. Problem Statement

Many Windows games — especially older titles and PC-exclusive applications — lack native touchscreen or virtual controller support. Handheld gaming PCs (Asus ROG Flow Z13, ROG Ally, Lenovo Legion Go, Steam Deck in Windows mode, MSI Claw) have touchscreens but no built-in on-screen controller. Users must either:

- Connect a physical gamepad, defeating the handheld portability advantage
- Use the Windows on-screen keyboard, which is inadequate for gaming
- Rely on third-party solutions that are often abandoned, poorly designed, or paid

**vcon** fills this gap as a free, open-source, lightweight overlay with first-class touch support and full layout customization.

---

## 3. Goals & Non-Goals

### In Scope (v1)

- Responsive, low-latency virtual controller overlay over windowed and borderless-fullscreen games
- Xbox 360 controller emulation via ViGEmBus
- Direct keyboard/mouse injection for games that don't recognize controllers
- Full customization of control layout, size, position, opacity, and key bindings
- Anchor-based positioning (controls anchored to screen edges) for resolution independence
- Taskbar-aware layout — controls offset above the Windows taskbar
- Switchable profiles for different games and use cases
- In-app visual layout editor with drag-to-move and resize handles
- Profile snapshot, save, and discard workflow
- Locked default profile with automatic cloning on edit
- Profile cloning
- System tray integration with full profile management, input mode switching, and driver installation
- Global hotkeys
- Clean, maintainable C# / .NET 8 codebase following .NET conventions

### Out of Scope (v1)

- Fullscreen exclusive game support (requires render pipeline hooking; architecturally different)
- Linux / Steam Deck native (Proton) support
- Physical gamepad remapping (physical controller → different output)
- Macro recording or playback
- Network streaming / remote play integration
- Anti-cheat bypass or evasion of any kind

---

## 4. Target Audience

| Segment | Need |
|:---|:---|
| **Handheld PC gamers** (ROG Ally, Legion Go, Steam Deck Windows, MSI Claw) | On-screen controls for games without native touch or gamepad support |
| **Accessibility users** | On-screen input method when physical controllers are difficult to use |
| **Legacy game players** | Controller emulation for titles that only accept keyboard input |
| **Developers and testers** | Quick virtual gamepad for testing controller support without physical hardware |

---

## 5. Functional Requirements

### FR-1: Overlay Window System

| ID | Requirement | Priority |
|:---|:---|:---|
| FR-1.1 | Render a transparent, borderless WPF window that floats above all other windows | P0 |
| FR-1.2 | Window must NOT steal focus from the foreground application (Win32 `WS_EX_NOACTIVATE`) | P0 |
| FR-1.3 | Window must use `WS_EX_TOPMOST` to remain visible over borderless-fullscreen games | P0 |
| FR-1.4 | Non-control areas of the overlay must be click-through (input passes to game beneath) | P0 |
| FR-1.5 | Support per-monitor DPI awareness | P1 |
| FR-1.6 | Support multi-touch input for simultaneous button presses | P0 |
| FR-1.7 | Adjustable global opacity (10%–100%) | P1 |
| FR-1.8 | Show/hide overlay via configurable global hotkey | P0 |

### FR-2: Virtual Controller Controls

The overlay renders the following interactive control types:

| Control | Description | Input Behavior |
|:---|:---|:---|
| **Button** | Circular or rounded-rectangle touch target (A, B, X, Y, LB, RB, Start, Back, Guide) | Binary press/release. Shape controlled by `cornerRadius` in StyleInfo — omit for circle, set small value for rounded rectangle. |
| **Analog Stick** | Circular touch zone with draggable thumb indicator | Continuous X/Y axes (−1.0 to +1.0) with configurable dead zone |
| **Trigger** | Rectangular touch zone (LT, RT) | Binary press for v1; analog via vertical drag in future |
| **D-Pad** | 4-directional or 8-directional pad | Discrete directional presses |

The **default layout** mirrors an Xbox 360 controller: Left Stick, Right Stick, D-Pad, A/B/X/Y, LB/RB, LT/RT, Start, Back, Guide. All controls are positioned using anchor-based coordinates to remain resolution-independent and taskbar-aware.

### FR-3: Input Emulation

| ID | Requirement | Priority |
|:---|:---|:---|
| FR-3.1 | **Xbox 360 mode**: Create a virtual Xbox 360 controller via ViGEmBus. All button, stick, and trigger inputs produce authentic XInput reports. | P0 |
| FR-3.2 | **Keyboard/Mouse mode**: Map each virtual control to one or more keystrokes or mouse actions. Inject via Win32 `SendInput`. | P0 |
| FR-3.3 | Input mode is per-profile (each profile declares its emulation mode) | P0 |
| FR-3.4 | Graceful degradation: if ViGEmBus driver is not installed, disable Xbox mode and display a clear warning with install instructions. Offer one-click install from tray menu. | P1 |
| FR-3.5 | Support key modifier combinations (e.g., Ctrl+Shift+S) in keyboard bindings | P2 |

### FR-4: Profile System

| ID | Requirement | Priority |
|:---|:---|:---|
| FR-4.1 | Profiles stored as individual JSON files in a user-writable directory (`%APPDATA%/vcon/profiles/`) | P0 |
| FR-4.2 | Each profile defines: name, input mode, control layout (positions with anchors, sizes, styles with corner radius), bindings, opacity, and scale | P0 |
| FR-4.3 | Ship with a default "Xbox Standard" profile (locked from in-app editing) | P0 |
| FR-4.4 | Switch active profile via system tray context menu | P0 |
| FR-4.5 | Switch active profile via configurable hotkey | P1 |
| FR-4.6 | Clone any profile via tray menu or programmatically | P1 |
| FR-4.7 | Open profiles directory in Windows Explorer from tray menu | P1 |
| FR-4.8 | Import/export profiles as standalone JSON files | P2 |
| FR-4.9 | Auto-switch profile based on foreground application executable name | P2 |

### FR-5: Layout Editor

| ID | Requirement | Priority | Status |
|:---|:---|:---|:---|
| FR-5.1 | Toggle into "Edit Mode" via hotkey (F10) or tray menu | P1 | Implemented |
| FR-5.2 | In edit mode: drag controls to reposition; resize via bottom-right grip handle | P1 | Implemented |
| FR-5.3 | In edit mode: input emulation is disabled (prevents accidental game inputs) | P1 | Implemented |
| FR-5.4 | In edit mode: visual indicators — cyan border overlays and resize grips on every control | P1 | Implemented |
| FR-5.5 | Explicit save/discard via tray menu Profiles submenu (not auto-save) | P1 | Implemented |
| FR-5.6 | JSON snapshot taken on entering edit mode; discard restores from snapshot | P1 | Implemented |
| FR-5.7 | Default "xbox-standard" profile is locked; entering edit mode auto-clones it | P1 | Implemented |
| FR-5.8 | Anchor-aware drag: coordinate conversion respects each control's horizontal and vertical anchor | P1 | Implemented |
| FR-5.9 | Stick resize updates `Radius` as well as `Width`/`Height` | P1 | Implemented |
| FR-5.10 | In edit mode: optional visual grid and alignment guides | P2 | Not yet |
| FR-5.11 | Add or remove controls from the layout | P2 | Not yet |

### FR-6: System Tray & Application Lifecycle

| ID | Requirement | Priority |
|:---|:---|:---|
| FR-6.1 | System tray icon with context menu: Show/Hide, Edit Mode, Input Mode, Profiles submenu (with save/discard/clone/open-dir), Install ViGEmBus, Game Controllers, Exit | P0 |
| FR-6.2 | Minimize to tray; no taskbar presence when overlay is active | P1 |
| FR-6.3 | Optional: launch at Windows startup | P2 |
| FR-6.4 | Single-instance enforcement via named mutex | P1 |

---

## 6. Non-Functional Requirements

### NFR-1: Performance

| ID | Requirement | Target |
|:---|:---|:---|
| NFR-1.1 | Input-to-emulation latency | < 5 ms from touch event to ViGEm report or `SendInput` call |
| NFR-1.2 | Memory footprint | < 80 MB working set |
| NFR-1.3 | CPU usage (idle, overlay visible) | < 1% |
| NFR-1.4 | CPU usage (active input) | < 3% |
| NFR-1.5 | Startup time | < 2 seconds to overlay visible |
| NFR-1.6 | Game frame rate impact | Zero dropped frames attributable to the overlay |

### NFR-2: Reliability

- Must not crash when ViGEmBus driver is absent; degrade gracefully to keyboard-only mode
- Must recover cleanly when the game process exits or display mode changes
- Profile corruption must not prevent app launch; fall back to the built-in default profile
- Unhandled exceptions are logged and surfaced via a non-modal notification, not a crash dialog

### NFR-3: Compatibility

- **OS**: Windows 10 21H2+ and Windows 11
- **Runtime**: .NET 8.0 LTS (self-contained deployment; no user-facing runtime dependency)
- **Display**: Single-monitor and multi-monitor configurations; per-monitor DPI
- **Game display modes**: Windowed and Borderless Fullscreen. **Not** fullscreen exclusive (see §9 Risks)
- **Input devices**: Touchscreens and mice. Pen input as a stretch goal.

### NFR-4: Usability

- First-time setup completes in under 2 minutes (excluding ViGEmBus driver install)
- All primary actions reachable within 2 interactions from the system tray
- Visual design consistent with Xbox controller aesthetic (green accent, ABXY button colors, rounded controls)

---

## 7. Technical Architecture

### 7.1 Technology Stack

| Component | Choice | Rationale |
|:---|:---|:---|
| **Runtime** | **.NET 8.0 LTS** | Current LTS release; full WPF support; modern C# features; self-contained publish eliminates user-facing runtime dependency. Chosen over .NET Framework 4.8 for performance, long-term support, and ARM64 readiness. |
| **UI Framework** | **WPF** (Windows Presentation Foundation) | Native transparency and layered window support, GPU-accelerated rendering, built-in multi-touch via `Touch` events, per-monitor DPI awareness, rich styling and control templating. Superior to WinForms for overlay scenarios. |
| **Controller Emulation** | **[Nefarius.ViGEm.Client](https://github.com/nefarius/ViGEm.Client.Net)** (NuGet) | Mature .NET wrapper for the ViGEmBus kernel driver; supports Xbox 360 and DualShock 4 virtual device creation. Widely used in the ecosystem (DS4Windows, etc.). |
| **Input Injection** | **Win32 `SendInput`** via P/Invoke (`user32.dll`) | Low-level, low-latency keyboard and mouse injection with no additional dependencies. |
| **MVVM** | **[CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)** (NuGet) | Lightweight source-generated MVVM toolkit; reduces boilerplate (ObservableProperty, RelayCommand) without a heavyweight framework. |
| **Serialization** | **System.Text.Json** | BCL-included; fast; source-generator support for AOT-friendly serialization. No additional dependency. |
| **Logging** | **Microsoft.Extensions.Logging** + **Serilog** (file sink) | Structured logging for diagnostics; file sink for post-mortem analysis. |
| **DI Container** | **Microsoft.Extensions.DependencyInjection** | Standard .NET DI; already transitively referenced by logging and hosting packages. |

### 7.2 Solution Structure

```
vcon/
├── src/
│   ├── Vcon.Core/                          # Shared abstractions, models, configuration
│   │   ├── Abstractions/
│   │   │   ├── IInputEmulator.cs           # Input emulation contract
│   │   │   ├── IProfileManager.cs          # Profile CRUD, switching, cloning
│   │   │   └── IControllerStateObserver.cs # Optional observation hook
│   │   ├── Models/
│   │   │   ├── ControllerState.cs          # Full controller state snapshot
│   │   │   ├── ControllerProfile.cs        # Serializable profile definition
│   │   │   ├── ControlDefinition.cs        # Single control's layout and binding
│   │   │   ├── PositionInfo.cs             # Anchor-relative normalized position
│   │   │   ├── SizeInfo.cs                 # Control dimensions (width, height, radius)
│   │   │   ├── StyleInfo.cs                # Fill, stroke, corner radius
│   │   │   ├── HorizontalAnchor.cs         # Left / Right enum
│   │   │   ├── VerticalAnchor.cs           # Top / Bottom enum
│   │   │   ├── InputBinding.cs             # Maps a control to XInput button or key
│   │   │   ├── DirectionalBinding.cs       # WASD-style bindings for sticks/D-pad
│   │   │   └── AppSettings.cs              # Global application settings
│   │   ├── Configuration/
│   │   │   └── ProfileSerializer.cs        # JSON round-trip for profiles and settings
│   │   └── Vcon.Core.csproj
│   │
│   ├── Vcon.Input/                         # Input emulation implementations
│   │   ├── XInput/
│   │   │   ├── ViGEmEmulator.cs            # IInputEmulator → ViGEmBus Xbox 360
│   │   │   └── ViGEmAvailability.cs        # Runtime driver detection
│   │   ├── Keyboard/
│   │   │   └── SendInputEmulator.cs        # IInputEmulator → Win32 SendInput
│   │   ├── Native/
│   │   │   └── NativeMethods.cs            # P/Invoke declarations
│   │   ├── InputEmulatorFactory.cs         # Resolves correct emulator for a profile's mode
│   │   └── Vcon.Input.csproj
│   │
│   ├── Vcon.Overlay/                       # WPF application host (thin)
│   │   ├── App.xaml / App.xaml.cs          # Startup, DI composition, single-instance
│   │   ├── Windows/
│   │   │   └── OverlayWindow.xaml(.cs)     # Transparent, topmost, no-activate window
│   │   │                                   # + edit mode decorators, drag/resize handlers
│   │   ├── Controls/
│   │   │   ├── VirtualButton.xaml(.cs)     # Face buttons, bumpers, start/back/guide
│   │   │   ├── VirtualStick.xaml(.cs)      # Analog stick with touch-drag tracking
│   │   │   ├── VirtualTrigger.xaml(.cs)    # LT / RT
│   │   │   └── VirtualDPad.xaml(.cs)       # Directional pad
│   │   ├── ViewModels/
│   │   │   ├── OverlayViewModel.cs         # Main state management and input dispatch
│   │   │   └── EditorViewModel.cs          # Layout editing: snapshot, save, discard,
│   │   │                                   # auto-clone for locked profiles
│   │   ├── Services/
│   │   │   ├── OverlayWindowService.cs     # Win32 window-style interop
│   │   │   ├── ProfileManager.cs           # IProfileManager file-based implementation
│   │   │   ├── GlobalHotkeyService.cs      # RegisterHotKey-based global hotkeys
│   │   │   └── TrayIconService.cs          # System tray NotifyIcon + context menu
│   │   ├── Resources/
│   │   │   ├── Styles/                     # Control templates, brushes, colors
│   │   │   └── Icons/                      # Application and tray icons
│   │   └── Vcon.Overlay.csproj
│   │
│   └── Vcon.sln
│
├── tests/
│   ├── Vcon.Core.Tests/
│   ├── Vcon.Input.Tests/
│   └── Vcon.Overlay.Tests/
│
├── profiles/                               # Default profile templates (copied on first run)
│   └── xbox-standard.json
│
├── Directory.Build.props                   # Centralized version and shared build properties
├── Directory.Packages.props                # Centralized NuGet versions
├── Makefile                                # build, test, run, clean, lint, fmt, help, etc.
├── DESIGN.md
├── README.md
├── CHANGELOG.md
├── AGENTS.md
└── LICENSE
```

### 7.3 Component Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                   Vcon.Overlay (WPF Host)                     │
│                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────────────┐ │
│  │OverlayWindow │  │ TrayIcon     │  │ GlobalHotkey       │ │
│  │  + Controls  │  │   Service    │  │   Service          │ │
│  │  + EditMode  │  │              │  │                    │ │
│  └──────┬───────┘  └──────┬───────┘  └────────┬───────────┘ │
│         │                 │                    │             │
│  ┌──────▼─────────────────▼────────────────────▼──────────┐  │
│  │          OverlayViewModel + EditorViewModel            │  │
│  │  (controller state, input dispatch, edit mode,         │  │
│  │   snapshot/save/discard, profile locking/cloning)      │  │
│  └───────────────────────┬────────────────────────────────┘  │
│                          │                                   │
│  ┌───────────────────────▼────────────────────────────────┐  │
│  │                 ProfileManager                         │  │
│  │  (load / save / switch / clone profiles, file I/O)     │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────┬───────────────────────────────┘
                               │ depends on
┌──────────────────────────────▼───────────────────────────────┐
│                       Vcon.Input                              │
│                                                              │
│  ┌──────────────────────┐    ┌─────────────────────────────┐ │
│  │  ViGEmEmulator       │    │  SendInputEmulator          │ │
│  │  (Xbox 360 reports)  │    │  (keyboard/mouse injection) │ │
│  └──────────┬───────────┘    └──────────────┬──────────────┘ │
│             │ implements                    │ implements      │
└─────────────┼───────────────────────────────┼────────────────┘
              │                               │
┌─────────────▼───────────────────────────────▼────────────────┐
│                       Vcon.Core                               │
│                                                              │
│  ┌────────────────────┐    ┌───────────────────────────────┐ │
│  │ IInputEmulator     │    │ ControllerState               │ │
│  │ IProfileManager    │    │ ControllerProfile             │ │
│  │                    │    │ ControlDefinition             │ │
│  │                    │    │ PositionInfo (with anchors)   │ │
│  │                    │    │ SizeInfo / StyleInfo          │ │
│  │                    │    │ InputBinding / AppSettings    │ │
│  └────────────────────┘    └───────────────────────────────┘ │
└──────────────────────────────────────────────────────────────┘
              │                               │
              ▼                               ▼
    ViGEmBus Driver (kernel)       Windows Input Subsystem
```

**Dependency direction** is strictly top-down: Overlay → Input → Core. Core has zero upward dependencies. Input depends only on Core abstractions. Overlay composes everything via DI.

### 7.4 Key Abstractions

```csharp
public interface IInputEmulator : IDisposable
{
    bool IsAvailable { get; }
    void Connect();
    void Disconnect();
    void SubmitState(ControllerState state);
}
```

```csharp
public interface IProfileManager
{
    Task<IReadOnlyList<ControllerProfile>> GetAllProfilesAsync(CancellationToken ct = default);
    Task<ControllerProfile?> LoadProfileAsync(string profileId, CancellationToken ct = default);
    Task SaveProfileAsync(ControllerProfile profile, CancellationToken ct = default);
    Task<bool> DeleteProfileAsync(string profileId, CancellationToken ct = default);
    Task<ControllerProfile> CloneProfileAsync(string sourceId, string? newName = null, CancellationToken ct = default);
    ControllerProfile ActiveProfile { get; }
    string ProfilesDirectory { get; }
    Task SwitchProfileAsync(string profileId, CancellationToken ct = default);
    event EventHandler<ControllerProfile>? ActiveProfileChanged;
}
```

```csharp
public sealed class ControllerProfile
{
    public string Id { get; set; }
    public string Name { get; set; }
    public InputMode Mode { get; set; }       // XInput or Keyboard
    public float Opacity { get; set; }        // 0.1 – 1.0
    public float Scale { get; set; }          // Relative to reference resolution
    public List<ControlDefinition> Controls { get; set; }
}

public enum InputMode { XInput, Keyboard }
```

```csharp
public sealed class ControlDefinition
{
    public string Id { get; set; }
    public ControlType Type { get; set; }     // Button, Stick, Trigger, DPad
    public string Label { get; set; }
    public PositionInfo Position { get; set; } // Anchor-relative normalized coordinates
    public SizeInfo Size { get; set; }         // Device-independent pixels at 1920×1080 reference
    public StyleInfo? Style { get; set; }      // Fill, stroke, corner radius
    public InputBinding Binding { get; set; }
    public float DeadZone { get; set; }        // Sticks only
}

public enum ControlType { Button, Stick, Trigger, DPad }
```

```csharp
public sealed class PositionInfo
{
    public double X { get; set; }               // Normalized offset (0.0–1.0) from anchor edge
    public double Y { get; set; }               // Normalized offset (0.0–1.0) from anchor edge
    public HorizontalAnchor HAnchor { get; set; } = HorizontalAnchor.Left;
    public VerticalAnchor VAnchor { get; set; }   = VerticalAnchor.Top;
}

public enum HorizontalAnchor { Left, Right }
public enum VerticalAnchor { Top, Bottom }
```

```csharp
public sealed class StyleInfo
{
    public string? Fill { get; set; }           // Hex color (e.g. "#107C10")
    public string? Stroke { get; set; }         // Hex color
    public double? CornerRadius { get; set; }   // Reference pixels; null → auto-circle
}
```

### 7.5 Anchor-Based Positioning System

Control positions use **normalized coordinates (0.0–1.0)** measured from a configurable screen edge. This replaces the original top-left-only origin, enabling layouts that remain correct across different screen resolutions and taskbar configurations.

**How it works at runtime** (`OverlayWindow.PositionControl`):

1. The screen dimensions and taskbar insets are computed from `SystemParameters.PrimaryScreenWidth/Height` and `SystemParameters.WorkArea`.
2. For each control, the anchor determines which Canvas attached property is set:
   - `HAnchor.Left` → `Canvas.SetLeft(element, pos.X * screenWidth + taskbarInsets.Left)`
   - `HAnchor.Right` → `Canvas.SetRight(element, pos.X * screenWidth + taskbarInsets.Right)`
   - `VAnchor.Top` → `Canvas.SetTop(element, pos.Y * screenHeight + taskbarInsets.Top)`
   - `VAnchor.Bottom` → `Canvas.SetBottom(element, pos.Y * screenHeight + taskbarInsets.Bottom)`

This ensures bottom-anchored controls automatically sit above the Windows taskbar, and right-anchored controls stay at the right edge regardless of screen width.

### 7.6 Edit Mode Architecture

Edit mode allows users to visually drag and resize controls, then explicitly save or discard changes.

**Key classes:**

- **`EditorViewModel`**: Manages the editing lifecycle — snapshot, save, discard, locked-profile detection, auto-clone.
- **`OverlayViewModel`**: Delegates `ToggleEditMode()` to `EditorViewModel.StartEditingAsync()`. Syncs `IsEditMode` from `EditorViewModel.IsEditing` via `PropertyChanged`.
- **`OverlayWindow`**: Creates visual edit overlays (cyan borders + resize grips) and handles drag/resize mouse events with anchor-aware coordinate conversion.

**Edit mode flow:**

```
User presses F10 (or tray → Edit Mode)
    │
    ▼
OverlayViewModel.ToggleEditMode()
    │
    ▼
EditorViewModel.StartEditingAsync()
    ├── Is profile "xbox-standard"?
    │   ├── Yes: CloneProfileAsync → SwitchProfileAsync → layout reloads
    │   └── No: continue
    ├── Snapshot active profile as JSON
    └── IsEditing = true → OverlayWindow.EnterEditMode()
            │
            ├── For each control: create edit overlay (Grid with cyan Border + resize grip)
            ├── Attach drag handlers to overlays
            └── Attach resize handlers to grip elements

User drags a control
    │
    ▼
OverlayWindow: MouseMove updates Canvas.Left/Right/Top/Bottom for both element and overlay
    │
    ▼
OverlayWindow: MouseUp converts canvas position back to normalized anchor-relative coordinates
    │
    ▼
EditorViewModel.MoveControl(id, normX, normY) — mutates ControlDefinition.Position in memory

User clicks "Save Profile" in tray
    │
    ▼
EditorViewModel.SaveAndStopAsync()
    ├── ProfileManager.SaveProfileAsync(profile) — writes JSON to disk
    ├── Clear snapshot
    └── IsEditing = false → OverlayWindow.ExitEditMode() → remove overlays, reload layout

User clicks "Discard Changes" in tray
    │
    ▼
EditorViewModel.DiscardAndStopAsync()
    ├── Deserialize snapshot → restore Controls, Opacity, Scale on active profile
    ├── SaveProfileAsync (persist the restored state)
    ├── Fire LayoutReloadRequested → OverlayWindow reloads layout
    ├── Clear snapshot
    └── IsEditing = false → OverlayWindow.ExitEditMode()
```

**Coordinate conversion (drag):**
- Left-anchored: `normX = (Canvas.GetLeft(element) - taskbarInsets.Left) / screenWidth`
- Right-anchored: `normX = (Canvas.GetRight(element) - taskbarInsets.Right) / screenWidth`
- Top-anchored: `normY = (Canvas.GetTop(element) - taskbarInsets.Top) / screenHeight`
- Bottom-anchored: `normY = (Canvas.GetBottom(element) - taskbarInsets.Bottom) / screenHeight`

**Resize conversion:** Element pixel dimensions are divided by the profile `Scale` factor to produce reference-pixel values stored in `SizeInfo`. For stick controls, `SizeInfo.Radius` is also updated (`min(width, height) / 2`).

**Locked profile behavior:** `EditorViewModel.IsProfileLocked` returns `true` when `profile.Id == "xbox-standard"` (case-insensitive). If locked, `StartEditingAsync` calls `CloneProfileAsync` which generates a unique ID (`xbox-standard-copy`, `xbox-standard-copy-2`, etc.) and switches to the clone before taking the snapshot.

### 7.7 Profile Cloning

`ProfileManager.CloneProfileAsync` performs a deep copy by serializing the source profile to JSON and deserializing it back. The clone gets a unique ID generated by `GenerateUniqueId`:

1. Try `{sourceId}-copy`
2. If that file exists, try `{sourceId}-copy-2`, `{sourceId}-copy-3`, etc.
3. Set `Name` to `"{sourceName} (copy)"` (or the caller-provided `newName`)
4. Save clone to disk

### 7.8 Data Flow: Touch Event → Game Input

```
User Touch / Mouse Event (WPF Dispatcher Thread)
        │
        ▼
  VirtualButton / VirtualStick / VirtualTrigger / VirtualDPad
        │  Control interprets gesture: binary press, axis position, or direction
        │  (In edit mode: input events are NOT fired — overlays intercept mouse)
        │
        ▼
  OverlayViewModel.UpdateButton / UpdateStick / UpdateTrigger / UpdateDPad
        │  Mutates the shared ControllerState model
        │  (Suppressed when IsEditMode is true)
        │
        ▼
  IInputEmulator.SubmitState(controllerState)
        │
        ├─── ViGEmEmulator path ─────────────────── SendInputEmulator path ───┐
        │    Translate ControllerState to             Diff current state vs    │
        │    Xbox360Report (buttons, axes,            previous state. For      │
        │    triggers as native ViGEm types)          each transition:         │
        │    controller.SubmitReport(report)           • press → keyDown       │
        │         │                                    • release → keyUp       │
        │         ▼                                    • axis → WASD discrete  │
        │    ViGEmBus kernel driver                         │                  │
        │         │                                         ▼                  │
        │         ▼                                  Win32 SendInput            │
        │    XInput API                              (user32.dll)              │
        │    (game polls this)                       Windows message queue     │
        │                                            (game receives this)      │
        └─────────────────────────────────────────────────────────────────────┘
```

**Key design choice**: The entire hot path runs synchronously on the WPF dispatcher thread. Both `ViGEmClient.SubmitReport()` and `SendInput()` complete in under 1 ms, so dispatching to a background thread would add latency and synchronization cost without benefit. If profiling reveals jank, the submission step can be moved to a dedicated thread with a lock-free single-producer/single-consumer queue.

### 7.9 Threading Model

| Thread | Responsibility |
|:---|:---|
| **WPF Dispatcher (UI)** | Touch/mouse event handling, UI rendering, control state updates, emulator submission, edit mode drag/resize |
| **Global Hotkey Listener** | Runs on UI thread via `RegisterHotKey` + window message pump (`HwndSource`) |
| **Profile I/O** | Async file reads/writes on the thread pool; results marshaled back to dispatcher via `Dispatcher.InvokeAsync` |
| **Logging** | Serilog async sink writes to file on a background thread |

### 7.10 Overlay Window Implementation

The overlay uses specific Win32 extended window styles applied via `SetWindowLongPtr` in the window's `SourceInitialized` handler:

| Style | Hex | Purpose |
|:---|:---|:---|
| `WS_EX_NOACTIVATE` | `0x08000000` | Window does not become foreground on click; game retains focus |
| `WS_EX_TOOLWINDOW` | `0x00000080` | Hidden from Alt+Tab and taskbar |
| `WS_EX_TOPMOST` | `0x00000008` | Always rendered above other windows (also set via WPF `Topmost="True"`) |

**Click-through for non-control areas** is handled natively by WPF's hit-test model: with `AllowsTransparency="True"` and `Background="Transparent"` on the window, areas with no visible content pass all input (mouse and touch) through to the window below. Only controls with non-transparent backgrounds receive input. No `WS_EX_TRANSPARENT` toggling is needed.

**Overlay window XAML skeleton:**

```xml
<Window x:Class="Vcon.Overlay.Windows.OverlayWindow"
        AllowsTransparency="True"
        WindowStyle="None"
        Background="Transparent"
        Topmost="True"
        ShowInTaskbar="False"
        WindowState="Maximized">
    <Canvas x:Name="ControlCanvas">
        <!-- Controls positioned via Canvas.Left/Right/Top/Bottom based on anchors -->
        <!-- In edit mode: overlay Grid elements with cyan borders + resize grips -->
    </Canvas>
</Window>
```

**Multi-touch**: WPF delivers distinct `TouchDown`, `TouchMove`, and `TouchUp` events per touch point, each with a unique `TouchDevice.Id`. Controls handle their own touch device independently, enabling simultaneous button presses.

**Button shapes**: `VirtualButton` uses a WPF `Border` element (not `Ellipse`) to support both circular and rounded-rectangle shapes. The `CornerRadius` is set from `StyleInfo.CornerRadius` (scaled by the profile scale factor), or defaults to `min(width, height) / 2` to produce a circle for square buttons.

### 7.11 Tray Menu Structure

```
vcon — Virtual Controller Overlay
├── Show/Hide Overlay
├── Edit Mode
├── ──────────
├── Xbox Controller Mode     [✓ when active]
├── Keyboard Mode            [✓ when active]
├── ──────────
├── Profiles >
│   ├── Xbox Standard            [✓ when active]
│   ├── Xbox Standard (copy)     [✓ when active]
│   ├── ──────────
│   ├── Save Profile             [visible only in edit mode]
│   ├── Discard Changes          [visible only in edit mode]
│   ├── ──────────
│   ├── Clone Active Profile...
│   └── Open Profiles Directory
├── Install ViGEmBus Driver...   [visible only when driver missing]
├── ──────────
├── Game Controllers...
├── ──────────
└── Exit
```

The Profiles submenu is rebuilt dynamically each time the context menu opens (`menu.Opening` event) to reflect the current profile list and edit mode state.

### 7.12 Configuration Schema

**Global settings** stored at `%APPDATA%/vcon/settings.json`:

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

**Profile** stored at `%APPDATA%/vcon/profiles/{id}.json`:

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
      "position": { "x": 0.045, "y": 0.115, "hAnchor": "right", "vAnchor": "bottom" },
      "size": { "width": 56, "height": 56 },
      "style": { "fill": "#107C10", "stroke": "#0E6B0E" },
      "binding": { "xInput": "A", "keyboard": "Space" }
    },
    {
      "id": "left-stick",
      "type": "stick",
      "label": "",
      "position": { "x": 0.025, "y": 0.12, "hAnchor": "left", "vAnchor": "bottom" },
      "size": { "width": 0, "height": 0, "radius": 70 },
      "deadZone": 0.15,
      "binding": {
        "xInput": "LeftThumb",
        "keyboardDirectional": { "up": "W", "down": "S", "left": "A", "right": "D" }
      }
    },
    {
      "id": "left-bumper",
      "type": "button",
      "label": "lb",
      "position": { "x": 0.05, "y": 0.19, "hAnchor": "left", "vAnchor": "bottom" },
      "size": { "width": 50, "height": 65 },
      "style": { "fill": "#3A3A3A", "stroke": "#555555", "cornerRadius": 10 },
      "binding": { "xInput": "LeftShoulder", "keyboard": "Q" }
    }
  ]
}
```

**Design decisions on coordinates and sizing**:
- **Positions** use normalized coordinates (0.0–1.0) relative to an anchor edge, making layouts resolution-independent across devices. The `hAnchor` and `vAnchor` properties determine which screen edges the offsets are measured from.
- **Taskbar awareness**: When `vAnchor` is `bottom`, the offset is measured from the bottom of the usable work area (above the taskbar), not from the absolute screen bottom. Similarly, `taskbarInsets.Left/Right/Top` are added for other anchors.
- **Sizes** are in device-independent pixels at a 1920×1080 reference resolution. At runtime, sizes are scaled proportionally to the profile's `scale` factor.
- **Corner radius** in `StyleInfo` controls button shape. Omitted = auto-circle (`min(w,h)/2`). A small value like `10` creates a rounded rectangle. This is how bumpers (LB/RB) are rendered as rounded rectangles while face buttons (A/B/X/Y) remain circular.

---

## 8. Dependencies & Prerequisites

### Bundled / NuGet (resolved at build time)

| Dependency | Purpose |
|:---|:---|
| .NET 8.0 SDK | Build and self-contained publish |
| Nefarius.ViGEm.Client | ViGEmBus .NET wrapper for virtual Xbox 360 controller |
| CommunityToolkit.Mvvm | Source-generated MVVM (ObservableProperty, RelayCommand) |
| Microsoft.Extensions.DependencyInjection | DI container |
| Microsoft.Extensions.Logging | Logging abstractions |
| Serilog.Extensions.Logging + Serilog.Sinks.File | Structured file logging |

Versions are centralized in `Directory.Packages.props`. Use latest stable at time of implementation.

### User-Installed

| Dependency | Purpose | Fallback if Missing |
|:---|:---|:---|
| **[ViGEmBus Driver](https://github.com/nefarius/ViGEmBus/releases)** | Kernel-mode virtual gamepad driver | Xbox mode is disabled; keyboard mode still functions; app displays a clear warning. Installable from tray menu. |

### Build Tooling

| Tool | Purpose |
|:---|:---|
| .NET 8.0 SDK | `dotnet build`, `dotnet test`, `dotnet publish` |
| GNU Make (or compatible) | Makefile-based build shortcuts (`make build`, `make test`, `make run`, etc.) |

---

## 9. Risk Analysis & Mitigations

| Risk | Severity | Likelihood | Mitigation |
|:---|:---|:---|:---|
| **Anti-cheat software flags overlay or ViGEmBus** | High | Medium | Document known incompatible titles. ViGEmBus is widely used (DS4Windows, Steam Input) and allowlisted by most anti-cheat vendors. vcon does not inject into game processes. |
| **Fullscreen exclusive games hide the overlay** | High | High | Explicitly document this as an unsupported mode. Recommend borderless fullscreen. Windows 11 DWM composites all fullscreen games by default, so most modern setups work. |
| **WPF transparent window drops touch events** | Medium | Low | WPF `AllowsTransparency` has been reliable since .NET 4.5. Validate on target handhelds in Phase 1 before investing further. |
| **ViGEmBus project maintenance slows or stops** | Medium | Medium | Nefarius announced reduced ViGEmBus development in late 2023, though the driver remains functional and community-forked. The `IInputEmulator` abstraction isolates vcon from the driver; a replacement (community fork, vJoy, or custom HID driver) can be swapped without UI or profile changes. |
| **High input latency degrades gameplay** | High | Low | Synchronous single-thread hot path avoids queuing overhead. Profile with stopwatch instrumentation; target < 5 ms end-to-end. |
| **Multi-touch conflicts with Windows shell gestures** | Medium | Medium | Suppress Windows touch gestures on the overlay window via `SetGestureConfig` P/Invoke or per-window gesture configuration. Test edge-swipe and three-finger gestures on handhelds. |
| **`SendInput` blocked by UIPI** | Medium | Low | If the target game runs elevated and vcon does not, `SendInput` calls are silently dropped by Windows UIPI. Document: run vcon as Administrator when targeting elevated processes, or prefer Xbox emulation mode which is not affected by UIPI. |
| **Profile JSON corruption prevents launch** | Low | Low | Wrap profile deserialization in try/catch; log the error and fall back to the built-in default profile. Never overwrite a profile without writing to a `.tmp` file first and atomically renaming. |

---

## 10. Development Roadmap

### Phase 1: Ghost Window & Touch Proof-of-Concept ✅

**Goal**: Validate the core overlay technique on real hardware.

- Set up solution structure (`Vcon.Core`, `Vcon.Input`, `Vcon.Overlay`, test projects)
- Create transparent, topmost, no-activate WPF window
- Apply Win32 extended window styles (`WS_EX_NOACTIVATE`, `WS_EX_TOOLWINDOW`)
- Render a single test button; verify touch and mouse input while a game runs in the foreground
- Verify click-through on empty (transparent) areas
- Suppress Windows touch gestures on the overlay window
- Set up `Makefile` with `build`, `run`, `clean`, `test`, `lint`, `fmt`, `help` targets
- Set up `Directory.Build.props` with centralized versioning

### Phase 2: Xbox Controller Emulation ✅

**Goal**: Prove virtual controller emulation end-to-end.

- Integrate `Nefarius.ViGEm.Client`; implement `ViGEmEmulator : IInputEmulator`
- Implement `ControllerState` model
- Wire the test button to emit Xbox 360 "A" press via ViGEm
- Verify in Windows Game Controllers panel (`joy.cpl`) or a test game
- Implement `ViGEmAvailability` for runtime driver detection
- Add graceful degradation UI when ViGEmBus is not installed

### Phase 3: Full Controller Layout ✅

**Goal**: Render and wire the complete Xbox controller surface.

- Implement `VirtualButton`, `VirtualStick`, `VirtualDPad`, `VirtualTrigger` custom WPF controls
- Implement analog stick touch tracking: drag within circular zone maps to continuous X/Y axes
- Build the default "Xbox Standard" profile JSON with all controls positioned
- Wire all controls through `OverlayViewModel` → `ControllerState` → `IInputEmulator`
- Apply Xbox-inspired visual styling (ABXY colors, translucent rounded controls)
- Test multi-touch: verify 5+ simultaneous touches register correctly

### Phase 4: Keyboard/Mouse Injection Mode ✅

**Goal**: Support games that don't recognize XInput controllers.

- Implement `SendInputEmulator : IInputEmulator` using Win32 `SendInput` P/Invoke
- Implement state diffing: detect press/release transitions, emit corresponding `keyDown`/`keyUp`
- Map analog stick to discrete WASD (or configurable) directional keys
- Map triggers to key or mouse button presses
- Profile schema validated to include keyboard bindings for each control

### Phase 5: Profiles, Persistence & System Tray ✅

**Goal**: User-facing configuration management and application lifecycle.

- Implement `ProfileSerializer` (JSON round-trip with `System.Text.Json`)
- Implement `ProfileManager` (load, save, list, switch, clone, validate)
- Store settings and profiles under `%APPDATA%/vcon/`; copy defaults on first run
- Implement `TrayIconService` with full context menu (Show/Hide, Edit Mode, Input Mode, Profiles with save/discard/clone/open-dir, Install ViGEmBus, Game Controllers, Exit)
- Implement `GlobalHotkeyService` (show/hide overlay, toggle edit mode, cycle profiles)
- Single-instance enforcement via named `Mutex`
- DI composition in `App.xaml.cs`

### Phase 6: Layout Editor ✅

**Goal**: In-app visual layout customization.

- Implement edit mode toggle via hotkey and tray context menu
- Locked default profile ("xbox-standard") auto-clones on edit attempt
- JSON snapshot on enter; explicit save/discard via tray menu
- In edit mode: controls become draggable on the `Canvas` with anchor-aware coordinate conversion
- Resize handles on controls (bottom-right grip)
- Visual indicators for edit mode (cyan border overlays on every control)
- Input emulation paused during edit mode
- Anchor-aware position persistence and stick radius persistence on resize

### Phase 7: Polish & v1.0 Release

**Goal**: Release-quality build.

- Per-monitor DPI testing across common resolutions (1080p, 1200p, 1600p, 4K)
- Multi-touch stress testing on physical handheld hardware
- Performance profiling: latency instrumentation, CPU/memory measurement under load
- Error handling audit (missing driver, corrupt profiles, display mode changes, app crash recovery)
- Self-contained publish for **x64** and **ARM64**
- README with screenshots, install instructions, usage guide, known limitations
- Tag `v1.0.0`

---

## 11. Testing Strategy

| Layer | Framework | Scope |
|:---|:---|:---|
| **Vcon.Core** | xUnit | Unit tests: `ControllerState` manipulation, profile serialization round-trips (including anchor properties), `InputBinding` resolution, validation edge cases |
| **Vcon.Input** | xUnit | Unit tests: `SendInputEmulator` state-diffing logic (mock `SendInput` calls), `ViGEmAvailability` detection logic. `ViGEmEmulator` tested with mocked `IXbox360Controller` interface. |
| **Vcon.Overlay** | xUnit + NSubstitute | ViewModel unit tests: state transitions, profile switching, edit mode enter/save/discard lifecycle, `IsEditMode` sync between `EditorViewModel` and `OverlayViewModel`, input dispatch routing |
| **Integration** | Manual + scripted | End-to-end on physical hardware: touch input → ViGEm report visible in Game Controllers; keyboard injection verified in Notepad and a real game |
| **Performance** | BenchmarkDotNet | Micro-benchmarks for the hot path (state update → report submission). Target: < 5 ms p99. |

All automated tests run via `make test` → `dotnet test`. CI (when configured) gates on test pass + `dotnet format --verify-no-changes`.

---

## 12. Security Considerations

- **No admin required** for normal operation. ViGEmBus driver installation is a one-time admin action performed by the user outside vcon (or via the tray menu installer which prompts for UAC).
- **No hardcoded secrets**. Configuration files contain only layout and preference data.
- **UIPI awareness**: `SendInput` cannot inject into elevated (admin) processes from a non-elevated vcon instance. This is by Windows design. Xbox emulation mode (ViGEmBus) is unaffected by UIPI.
- **No process injection**: vcon does not hook, patch, or inject code into game processes. It operates entirely through OS-level input APIs.
- **Code signing**: Not required for v1 (personal project), but recommended before public distribution to avoid SmartScreen warnings.

---

## 13. Future Considerations

These items are out of scope for v1 but influence architectural decisions — abstractions and extension points are designed to accommodate them:

| Feature | Notes |
|:---|:---|
| **DualShock 4 emulation** | ViGEmBus supports DS4 device creation; `IInputEmulator` already accommodates additional implementations |
| **Analog triggers via vertical drag** | `VirtualTrigger` control can be extended to report float values based on touch drag distance |
| **Auto-profile switching by game** | Monitor foreground window process name; match against profile metadata field |
| **Macro and combo support** | Timed sequence of inputs triggered by a single press; requires a macro engine service |
| **Haptic/vibration feedback** | Windows touch haptic API on supported devices; ViGEm also supports force-feedback reports |
| **Radial menus** | Touch-and-hold to open a radial selector (weapon wheels, emotes, etc.) |
| **Community profile sharing** | Import/export via file; optional online repository |
| **Visual themes and skins** | Alternative control styles beyond Xbox aesthetic; user-provided SVG assets |
| **ARM64 native build** | .NET 8 supports ARM64 natively; relevant for Snapdragon-based handhelds |
| **Multiple simultaneous virtual controllers** | Enable local co-op with one physical + one virtual controller; requires multi-device ViGEm management |
| **Visual grid and snap-to-grid in edit mode** | Alignment guides to help position controls precisely |
| **Add/remove controls in edit mode** | Allow users to add new controls or remove existing ones from the layout |
