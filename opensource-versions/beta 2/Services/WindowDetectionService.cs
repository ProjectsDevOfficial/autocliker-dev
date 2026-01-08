using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using AutoClicker.Models;

namespace AutoClicker.Services
{
    public class WindowDetectionService
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public class WindowInfo
        {
            public IntPtr Handle { get; set; }
            public string Title { get; set; } = string.Empty;
            public Rectangle Rectangle { get; set; }
            public bool IsActive { get; set; }
            public string ProcessName { get; set; } = string.Empty;
        }

        public WindowInfo? GetActiveWindow()
        {
            IntPtr hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero) return null;

            return GetWindowInfo(hWnd);
        }

        public WindowInfo GetWindowInfo(IntPtr hWnd)
        {
            var info = new WindowInfo { Handle = hWnd };

            int length = GetWindowTextLength(hWnd);
            if (length > 0)
            {
                var sb = new StringBuilder(length + 1);
                GetWindowText(hWnd, sb, sb.Capacity);
                info.Title = sb.ToString();
            }

            if (GetWindowRect(hWnd, out RECT rect))
            {
                info.Rectangle = new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }

            info.IsActive = hWnd == GetForegroundWindow();

            try
            {
                var process = Process.GetProcessById(GetWindowProcessId(hWnd));
                info.ProcessName = process.ProcessName;
            }
            catch
            {
                info.ProcessName = "Unknown";
            }

            return info;
        }

        public List<WindowInfo> GetAllVisibleWindows()
        {
            var windows = new List<WindowInfo>();

            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    var info = GetWindowInfo(hWnd);
                    if (!string.IsNullOrEmpty(info.Title))
                    {
                        windows.Add(info);
                    }
                }
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        public List<WindowInfo> FindWindowsByTitle(string title)
        {
            var windows = GetAllVisibleWindows();
            return windows.FindAll(w => w.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        public List<WindowInfo> FindWindowsByProcess(string processName)
        {
            var windows = GetAllVisibleWindows();
            return windows.FindAll(w => w.ProcessName.Contains(processName, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsPointInWindow(Point point, IntPtr hWnd)
        {
            if (GetWindowRect(hWnd, out RECT rect))
            {
                return point.X >= rect.Left && point.X <= rect.Right &&
                       point.Y >= rect.Top && point.Y <= rect.Bottom;
            }
            return false;
        }

        public Point GetRelativePosition(Point absolutePosition, IntPtr hWnd)
        {
            if (GetWindowRect(hWnd, out RECT rect))
            {
                return new Point(absolutePosition.X - rect.Left, absolutePosition.Y - rect.Top);
            }
            return absolutePosition;
        }

        public Point GetAbsolutePosition(Point relativePosition, IntPtr hWnd)
        {
            if (GetWindowRect(hWnd, out RECT rect))
            {
                return new Point(relativePosition.X + rect.Left, relativePosition.Y + rect.Top);
            }
            return relativePosition;
        }

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private int GetWindowProcessId(IntPtr hWnd)
        {
            GetWindowThreadProcessId(hWnd, out uint processId);
            return (int)processId;
        }

        public bool WaitForWindow(string title, int timeoutMs = 5000)
        {
            var stopwatch = Stopwatch.StartNew();
            
            while (stopwatch.ElapsedMilliseconds < timeoutMs)
            {
                var windows = FindWindowsByTitle(title);
                if (windows.Count > 0)
                {
                    return true;
                }
                System.Threading.Thread.Sleep(100);
            }
            
            return false;
        }

        public bool ActivateWindow(IntPtr hWnd)
        {
            try
            {
                var process = Process.GetProcessById(GetWindowProcessId(hWnd));
                if (process.MainWindowHandle == hWnd)
                {
                    SetForegroundWindow(hWnd);
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
