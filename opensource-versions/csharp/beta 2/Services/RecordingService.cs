using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutoClicker.Models;

namespace AutoClicker.Services
{
    public class RecordingService : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        private bool _isRecording = false;
        private List<ClickSequence> _recordedActions = new List<ClickSequence>();
        private Stopwatch _recordingTimer = new Stopwatch();
        private DateTime _lastActionTime = DateTime.Now;

        public event EventHandler<ClickSequence>? ActionRecorded;
        public event EventHandler? RecordingStarted;
        public event EventHandler? RecordingStopped;

        public bool IsRecording => _isRecording;
        public List<ClickSequence> RecordedActions => new List<ClickSequence>(_recordedActions);
        public TimeSpan RecordingDuration => _recordingTimer.Elapsed;

        public void StartRecording()
        {
            if (_isRecording) return;

            _isRecording = true;
            _recordedActions.Clear();
            _recordingTimer.Restart();
            _lastActionTime = DateTime.Now;

            Task.Run(() => RecordingLoop());

            RecordingStarted?.Invoke(this, EventArgs.Empty);
        }

        public void StopRecording()
        {
            if (!_isRecording) return;

            _isRecording = false;
            _recordingTimer.Stop();

            RecordingStopped?.Invoke(this, EventArgs.Empty);
        }

        private async Task RecordingLoop()
        {
            while (_isRecording)
            {
                await Task.Delay(10);

                if (CheckMouseClick(out var clickType, out var position))
                {
                    var currentTime = DateTime.Now;
                    var delaySinceLastAction = (int)(currentTime - _lastActionTime).TotalMilliseconds;

                    var action = new ClickSequence
                    {
                        Position = position,
                        ClickType = clickType,
                        DelayAfter = delaySinceLastAction,
                        HoldDuration = 50
                    };

                    _recordedActions.Add(action);
                    _lastActionTime = currentTime;

                    ActionRecorded?.Invoke(this, action);
                }
            }
        }

        private bool CheckMouseClick(out ClickType clickType, out Point position)
        {
            clickType = ClickType.Left;
            position = Point.Empty;

            bool leftClick = (GetAsyncKeyState(0x01) & 0x8000) != 0;
            bool rightClick = (GetAsyncKeyState(0x02) & 0x8000) != 0;
            bool middleClick = (GetAsyncKeyState(0x04) & 0x8000) != 0;

            if (leftClick || rightClick || middleClick)
            {
                GetCursorPos(out POINT cursorPos);
                position = new Point(cursorPos.X, cursorPos.Y);

                if (leftClick) clickType = ClickType.Left;
                else if (rightClick) clickType = ClickType.Right;
                else if (middleClick) clickType = ClickType.Middle;

                return true;
            }

            return false;
        }

        public void ClearRecording()
        {
            _recordedActions.Clear();
            _recordingTimer.Reset();
        }

        public void Dispose()
        {
            StopRecording();
            _recordingTimer?.Stop();
        }
    }
}
