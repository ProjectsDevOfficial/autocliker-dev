using System;
using System.Drawing;
using System.Runtime.InteropServices;
using AutoClicker.Models;

namespace AutoClicker.Services
{
    public class MouseService
    {
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        private const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        private const uint MOUSEEVENTF_LEFTUP = 0x04;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const uint MOUSEEVENTF_RIGHTUP = 0x10;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x20;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x40;

        public static Point GetCursorPosition()
        {
            GetCursorPos(out POINT point);
            return new Point(point.X, point.Y);
        }

        public static void SetCursorPosition(Point position)
        {
            SetCursorPos(position.X, position.Y);
        }

        public static void Click(ClickType clickType, Point position, int holdDuration = 50)
        {
            SetCursorPosition(position);

            switch (clickType)
            {
                case ClickType.Left:
                    mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(holdDuration);
                    mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    break;
                case ClickType.Right:
                    mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(holdDuration);
                    mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    break;
                case ClickType.Middle:
                    mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                    System.Threading.Thread.Sleep(holdDuration);
                    mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                    break;
            }
        }

        public static void DoubleClick(ClickType clickType, Point position, int holdDuration = 50)
        {
            Click(clickType, position, holdDuration);
            System.Threading.Thread.Sleep(50);
            Click(clickType, position, holdDuration);
        }

        public static void TripleClick(ClickType clickType, Point position, int holdDuration = 50)
        {
            Click(clickType, position, holdDuration);
            System.Threading.Thread.Sleep(50);
            Click(clickType, position, holdDuration);
            System.Threading.Thread.Sleep(50);
            Click(clickType, position, holdDuration);
        }

        public static Point GetRandomizedPosition(Point basePosition, int xRange, int yRange)
        {
            var random = new Random();
            int offsetX = random.Next(-xRange, xRange + 1);
            int offsetY = random.Next(-yRange, yRange + 1);
            return new Point(basePosition.X + offsetX, basePosition.Y + offsetY);
        }
    }
}
