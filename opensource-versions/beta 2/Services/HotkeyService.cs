using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AutoClicker.Services
{
    public class HotkeyService : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private const uint MOD_NONE = 0x0000;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        private readonly Dictionary<int, Action> _hotkeys = new Dictionary<int, Action>();
        private readonly IntPtr _handle;
        private int _hotkeyId = 1;

        public HotkeyService(IntPtr handle)
        {
            _handle = handle;
        }

        public bool RegisterHotkey(string keyString, Action callback)
        {
            uint modifiers = MOD_NONE;
            uint virtualKey = 0;

            var parts = keyString.Split('+', StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var trimmedPart = part.Trim().ToUpper();
                switch (trimmedPart)
                {
                    case "CTRL":
                    case "CONTROL":
                        modifiers |= MOD_CONTROL;
                        break;
                    case "ALT":
                        modifiers |= MOD_ALT;
                        break;
                    case "SHIFT":
                        modifiers |= MOD_SHIFT;
                        break;
                    case "WIN":
                    case "WINDOWS":
                        modifiers |= MOD_WIN;
                        break;
                    default:
                        virtualKey = GetVirtualKeyCode(trimmedPart);
                        break;
                }
            }

            if (virtualKey == 0) return false;

            var id = _hotkeyId++;
            if (RegisterHotKey(_handle, id, modifiers, virtualKey))
            {
                _hotkeys[id] = callback;
                return true;
            }

            return false;
        }

        public void UnregisterAllHotkeys()
        {
            foreach (var id in _hotkeys.Keys)
            {
                UnregisterHotKey(_handle, id);
            }
            _hotkeys.Clear();
        }

        public bool IsKeyPressed(string keyString)
        {
            var virtualKey = GetVirtualKeyCode(keyString.ToUpper());
            return virtualKey != 0 && (GetAsyncKeyState((int)virtualKey) & 0x8000) != 0;
        }

        private uint GetVirtualKeyCode(string key)
        {
            return key switch
            {
                "F1" => 0x70,
                "F2" => 0x71,
                "F3" => 0x72,
                "F4" => 0x73,
                "F5" => 0x74,
                "F6" => 0x75,
                "F7" => 0x76,
                "F8" => 0x77,
                "F9" => 0x78,
                "F10" => 0x79,
                "F11" => 0x7A,
                "F12" => 0x7B,
                "SPACE" => 0x20,
                "ENTER" => 0x0D,
                "ESC" => 0x1B,
                "TAB" => 0x09,
                "BACKSPACE" => 0x08,
                "DELETE" => 0x2E,
                "HOME" => 0x24,
                "END" => 0x23,
                "PAGEUP" => 0x21,
                "PAGEDOWN" => 0x22,
                "INSERT" => 0x2D,
                "UP" => 0x26,
                "DOWN" => 0x28,
                "LEFT" => 0x25,
                "RIGHT" => 0x27,
                "NUMLOCK" => 0x90,
                "SCROLLLOCK" => 0x91,
                "CAPSLOCK" => 0x14,
                "PAUSE" => 0x13,
                "PRINTSCREEN" => 0x2A,
                _ when key.Length == 1 => (uint)char.ToUpper(key[0]),
                _ => 0
            };
        }

        public void Dispose()
        {
            UnregisterAllHotkeys();
        }
    }
}
