using System.Drawing;

namespace AutoClicker.Models
{
    public class ClickSettings
    {
        public int ClickInterval { get; set; } = 1000;
        public int ClickCount { get; set; } = 0;
        public bool InfiniteClicks { get; set; } = true;
        public ClickType ClickType { get; set; } = ClickType.Left;
        public ClickMode ClickMode { get; set; } = ClickMode.Single;
        public Point ClickPosition { get; set; } = new Point(0, 0);
        public bool UseCurrentPosition { get; set; } = true;
        public bool EnableRandomization { get; set; } = false;
        public int RandomDelayMin { get; set; } = 500;
        public int RandomDelayMax { get; set; } = 1500;
        public bool EnableHotkeys { get; set; } = true;
        public string StartHotkey { get; set; } = "F1";
        public string StopHotkey { get; set; } = "F2";
        public int RandomXRange { get; set; } = 10;
        public int RandomYRange { get; set; } = 10;
        public bool EnableSound { get; set; } = true;
        public bool MinimizeToTray { get; set; } = true;
        public List<ClickSequence> Sequences { get; set; } = new List<ClickSequence>();
        public int RepeatSequences { get; set; } = 1;
        public bool EnableHumanSimulation { get; set; } = false;
        public double HumanSpeedVariation { get; set; } = 0.2;
    }

    public enum ClickType
    {
        Left,
        Right,
        Middle
    }

    public enum ClickMode
    {
        Single,
        Double,
        Triple
    }

    public class ClickSequence
    {
        public Point Position { get; set; }
        public ClickType ClickType { get; set; }
        public int DelayAfter { get; set; } = 100;
        public int HoldDuration { get; set; } = 50;
    }
}
