using System.Runtime.InteropServices;

namespace Vcon.Input.Native;

/// <summary>Win32 P/Invoke declarations for keyboard and mouse input injection.</summary>
internal static partial class NativeMethods
{
    internal const uint INPUT_MOUSE = 0;
    internal const uint INPUT_KEYBOARD = 1;

    internal const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
    internal const uint KEYEVENTF_KEYUP = 0x0002;
    internal const uint KEYEVENTF_SCANCODE = 0x0008;

    internal const uint MOUSEEVENTF_MOVE = 0x0001;
    internal const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    internal const uint MOUSEEVENTF_LEFTUP = 0x0004;
    internal const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    internal const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    internal const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    internal const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
    internal const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

    internal static readonly int InputSize = Marshal.SizeOf<INPUT>();

    /// <summary>Injects keyboard, mouse, or hardware input events into the input stream.</summary>
    [LibraryImport("user32.dll", SetLastError = true)]
    internal static partial uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    #region Structs

    [StructLayout(LayoutKind.Sequential)]
    internal struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public nuint dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public nuint dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT mi;
        [FieldOffset(0)] public KEYBDINPUT ki;
        [FieldOffset(0)] public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct INPUT
    {
        public uint type;
        public InputUnion u;
    }

    #endregion

    #region VirtualKeyCode

    internal enum VirtualKeyCode : ushort
    {
        VK_LBUTTON = 0x01,
        VK_RBUTTON = 0x02,
        VK_MBUTTON = 0x04,
        VK_BACK = 0x08,
        VK_TAB = 0x09,
        VK_RETURN = 0x0D,
        VK_SHIFT = 0x10,
        VK_CONTROL = 0x11,
        VK_MENU = 0x12,
        VK_PAUSE = 0x13,
        VK_CAPITAL = 0x14,
        VK_ESCAPE = 0x1B,
        VK_SPACE = 0x20,
        VK_PRIOR = 0x21,
        VK_NEXT = 0x22,
        VK_END = 0x23,
        VK_HOME = 0x24,
        VK_LEFT = 0x25,
        VK_UP = 0x26,
        VK_RIGHT = 0x27,
        VK_DOWN = 0x28,
        VK_INSERT = 0x2D,
        VK_DELETE = 0x2E,
        VK_0 = 0x30,
        VK_1 = 0x31,
        VK_2 = 0x32,
        VK_3 = 0x33,
        VK_4 = 0x34,
        VK_5 = 0x35,
        VK_6 = 0x36,
        VK_7 = 0x37,
        VK_8 = 0x38,
        VK_9 = 0x39,
        VK_A = 0x41,
        VK_B = 0x42,
        VK_C = 0x43,
        VK_D = 0x44,
        VK_E = 0x45,
        VK_F = 0x46,
        VK_G = 0x47,
        VK_H = 0x48,
        VK_I = 0x49,
        VK_J = 0x4A,
        VK_K = 0x4B,
        VK_L = 0x4C,
        VK_M = 0x4D,
        VK_N = 0x4E,
        VK_O = 0x4F,
        VK_P = 0x50,
        VK_Q = 0x51,
        VK_R = 0x52,
        VK_S = 0x53,
        VK_T = 0x54,
        VK_U = 0x55,
        VK_V = 0x56,
        VK_W = 0x57,
        VK_X = 0x58,
        VK_Y = 0x59,
        VK_Z = 0x5A,
        VK_LWIN = 0x5B,
        VK_RWIN = 0x5C,
        VK_NUMPAD0 = 0x60,
        VK_NUMPAD1 = 0x61,
        VK_NUMPAD2 = 0x62,
        VK_NUMPAD3 = 0x63,
        VK_NUMPAD4 = 0x64,
        VK_NUMPAD5 = 0x65,
        VK_NUMPAD6 = 0x66,
        VK_NUMPAD7 = 0x67,
        VK_NUMPAD8 = 0x68,
        VK_NUMPAD9 = 0x69,
        VK_MULTIPLY = 0x6A,
        VK_ADD = 0x6B,
        VK_SUBTRACT = 0x6D,
        VK_DECIMAL = 0x6E,
        VK_DIVIDE = 0x6F,
        VK_F1 = 0x70,
        VK_F2 = 0x71,
        VK_F3 = 0x72,
        VK_F4 = 0x73,
        VK_F5 = 0x74,
        VK_F6 = 0x75,
        VK_F7 = 0x76,
        VK_F8 = 0x77,
        VK_F9 = 0x78,
        VK_F10 = 0x79,
        VK_F11 = 0x7A,
        VK_F12 = 0x7B,
        VK_LSHIFT = 0xA0,
        VK_RSHIFT = 0xA1,
        VK_LCONTROL = 0xA2,
        VK_RCONTROL = 0xA3,
        VK_LMENU = 0xA4,
        VK_RMENU = 0xA5,
    }

    #endregion

    #region ParseKeyName

    private static readonly Dictionary<string, VirtualKeyCode> KeyNameMap = BuildKeyNameMap();

    /// <summary>Maps a friendly key name (e.g. "Space", "W", "LeftMouseButton") to a virtual key code.</summary>
    internal static VirtualKeyCode? ParseKeyName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        if (KeyNameMap.TryGetValue(name, out var vk))
            return vk;

        if (name.Length == 1)
        {
            char c = char.ToUpperInvariant(name[0]);
            if (c is >= 'A' and <= 'Z')
                return (VirtualKeyCode)(0x41 + (c - 'A'));
            if (c is >= '0' and <= '9')
                return (VirtualKeyCode)(0x30 + (c - '0'));
        }

        return null;
    }

    private static Dictionary<string, VirtualKeyCode> BuildKeyNameMap() =>
        new(StringComparer.OrdinalIgnoreCase)
        {
            // Mouse buttons
            ["LeftMouseButton"] = VirtualKeyCode.VK_LBUTTON,
            ["RightMouseButton"] = VirtualKeyCode.VK_RBUTTON,
            ["MiddleMouseButton"] = VirtualKeyCode.VK_MBUTTON,
            ["LMB"] = VirtualKeyCode.VK_LBUTTON,
            ["RMB"] = VirtualKeyCode.VK_RBUTTON,
            ["MMB"] = VirtualKeyCode.VK_MBUTTON,

            // Control keys
            ["Backspace"] = VirtualKeyCode.VK_BACK,
            ["Tab"] = VirtualKeyCode.VK_TAB,
            ["Enter"] = VirtualKeyCode.VK_RETURN,
            ["Return"] = VirtualKeyCode.VK_RETURN,
            ["Shift"] = VirtualKeyCode.VK_SHIFT,
            ["LeftShift"] = VirtualKeyCode.VK_LSHIFT,
            ["RightShift"] = VirtualKeyCode.VK_RSHIFT,
            ["Control"] = VirtualKeyCode.VK_CONTROL,
            ["Ctrl"] = VirtualKeyCode.VK_CONTROL,
            ["LeftControl"] = VirtualKeyCode.VK_LCONTROL,
            ["LeftCtrl"] = VirtualKeyCode.VK_LCONTROL,
            ["RightControl"] = VirtualKeyCode.VK_RCONTROL,
            ["RightCtrl"] = VirtualKeyCode.VK_RCONTROL,
            ["Alt"] = VirtualKeyCode.VK_MENU,
            ["LeftAlt"] = VirtualKeyCode.VK_LMENU,
            ["RightAlt"] = VirtualKeyCode.VK_RMENU,
            ["Pause"] = VirtualKeyCode.VK_PAUSE,
            ["CapsLock"] = VirtualKeyCode.VK_CAPITAL,
            ["Escape"] = VirtualKeyCode.VK_ESCAPE,
            ["Esc"] = VirtualKeyCode.VK_ESCAPE,
            ["Space"] = VirtualKeyCode.VK_SPACE,

            // Navigation
            ["PageUp"] = VirtualKeyCode.VK_PRIOR,
            ["PageDown"] = VirtualKeyCode.VK_NEXT,
            ["End"] = VirtualKeyCode.VK_END,
            ["Home"] = VirtualKeyCode.VK_HOME,
            ["Insert"] = VirtualKeyCode.VK_INSERT,
            ["Delete"] = VirtualKeyCode.VK_DELETE,

            // Arrow keys
            ["Left"] = VirtualKeyCode.VK_LEFT,
            ["Up"] = VirtualKeyCode.VK_UP,
            ["Right"] = VirtualKeyCode.VK_RIGHT,
            ["Down"] = VirtualKeyCode.VK_DOWN,

            // Function keys
            ["F1"] = VirtualKeyCode.VK_F1,
            ["F2"] = VirtualKeyCode.VK_F2,
            ["F3"] = VirtualKeyCode.VK_F3,
            ["F4"] = VirtualKeyCode.VK_F4,
            ["F5"] = VirtualKeyCode.VK_F5,
            ["F6"] = VirtualKeyCode.VK_F6,
            ["F7"] = VirtualKeyCode.VK_F7,
            ["F8"] = VirtualKeyCode.VK_F8,
            ["F9"] = VirtualKeyCode.VK_F9,
            ["F10"] = VirtualKeyCode.VK_F10,
            ["F11"] = VirtualKeyCode.VK_F11,
            ["F12"] = VirtualKeyCode.VK_F12,
        };

    #endregion
}
