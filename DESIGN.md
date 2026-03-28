
# PRD: vcon (OnScreen Virtual Controller Overlay)

## 1. Executive Summary
**vcon** is a Windows-based utility written in C# that provides a customizable, on-screen virtual controller. It is designed to sit "always-on-top" of full-screen or windowed games, allowing users to input commands via touch or mouse. It bridges the gap for games lacking native controller support by offering both **XInput Emulation** and **Direct Keyboard/Mouse Injection**.

## 2. Target Audience
* Users on handheld Windows devices (like the ROG Ally, Steam Deck, or Legion Go) who want custom on-screen macros.
* Users with accessibility needs who require on-screen input methods.
* Gamers playing legacy titles that do not support modern gamepads.

## 3. Core Functional Requirements

### 3.1 Overlay Engine
* **Transparent UI:** The controller must support adjustable opacity and a "click-through" mode for visibility.
* **Always-on-Top:** Must utilize `WS_EX_TOPMOST` and `WS_EX_NOACTIVATE` window styles to ensure the game remains the "Active" window while the overlay receives touch/mouse events.
* **Layout Editor:** A way to move/resize buttons and joysticks on the screen.

### 3.2 Input Emulation Modes
* **Xbox Controller Mode (XInput):**
    * Virtualize a standard Xbox 360/One controller.
    * Requires integration with a driver-level wrapper (e.g., **ViGEmBus** via the `ViGEmClient` .NET library).
* **Keyboard/Mouse Mode:**
    * Map virtual buttons to specific keystrokes (e.g., "A" button = `Spacebar`).
    * Inject inputs directly into the Windows input stream (using `SendInput` or `InputSimulator`).

### 3.3 Dynamic Switching
* Allow users to switch between profiles (e.g., "FPS Profile" vs. "Racing Profile") via a tray icon or a hidden "menu" button on the overlay.

---

## 4. Technical Stack & Dependencies

| Component | Technology |
| :--- | :--- |
| **Language** | C# / .NET 8.0 (or .NET Framework 4.8 for better legacy compatibility) |
| **UI Framework** | **WPF** (Windows Presentation Foundation) - chosen for its superior transparency and layering capabilities over WinForms. |
| **Controller Emulation** | **ViGEmClient** (C# wrapper for ViGEmBus driver). |
| **Input Injection** | **Windows API (`user32.dll`)** specifically `SendInput`. |
| **Configuration** | JSON-based files for button mappings and layouts. |

---

## 5. System Architecture (High-Level)

1.  **UI Layer (WPF):** Handles the rendering of the buttons. Uses "Non-Client" hit testing so the window doesn't take focus.
2.  **Input Logic Layer:** A mapping engine that translates a "Button Down" event from the UI into either a `ViGEm` report or a `SendInput` call.
3.  **Driver Interface:** The bridge to the ViGEmBus driver to trick Windows into seeing a hardware device.



---

## 6. Development Roadmap (MVP)

### Phase 1: The "Ghost" Window
* Create a WPF window that is transparent and stays on top.
* Implement `WindowStyles` to ensure clicking the window does not minimize a full-screen game.

### Phase 2: Virtual Device Integration
* Implement the ViGEmBus client.
* Prove of Concept: A single on-screen button that, when pressed, makes Windows see "Xbox Controller Button A" pressed.

### Phase 3: Keyboard Injection
* Implement `SendInput` for keyboard emulation.
* Create a toggle to switch the button from "Xbox Mode" to "Keyboard Mode."

### Phase 4: Styling & Persistence
* Add the "Xbox" aesthetic (A/B/X/Y colors, Joysticks).
* Save/Load button positions to a `settings.json` file.

---

## 7. Critical Challenges & Considerations
* **Anti-Cheat Software:** Some games (like Valorant or Destiny 2) may flag virtual input drivers or "Always-on-top" overlays as suspicious.
* **Focus Issues:** Using `WS_EX_NOACTIVATE` is critical. If the overlay takes focus, the game will likely stutter or minimize.
* **Driver Requirement:** Users will need to install the **ViGEmBus** driver separately for the Xbox emulation to function.
