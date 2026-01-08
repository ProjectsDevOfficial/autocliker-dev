using System;
using System.Drawing;

namespace AutoClicker.Models
{
    public class AdvancedSettings
    {
        public bool EnableColorDetection { get; set; } = false;
        public Color TargetColor { get; set; } = Color.Red;
        public int ColorTolerance { get; set; } = 10;
        public bool EnableImageRecognition { get; set; } = false;
        public string? ImagePath { get; set; }
        public double ImageMatchThreshold { get; set; } = 0.8;
        public bool EnableWindowDetection { get; set; } = false;
        public string? TargetWindowTitle { get; set; }
        public bool EnableConditionalClicking { get; set; } = false;
        public string? ConditionScript { get; set; }
        public bool EnableSmartDelay { get; set; } = false;
        public int MinSmartDelay { get; set; } = 100;
        public int MaxSmartDelay { get; set; } = 1000;
        public bool EnableAdaptiveSpeed { get; set; } = false;
        public double SpeedAdaptationRate { get; set; } = 0.1;
        public bool EnableBreaks { get; set; } = false;
        public int BreakInterval { get; set; } = 60;
        public int BreakDuration { get; set; } = 5000;
        public bool EnableLogging { get; set; } = false;
        public string LogFilePath { get; set; } = "autoclicker.log";
        public bool EnableStatistics { get; set; } = false;
        public ClickStatistics Statistics { get; set; } = new ClickStatistics();
    }

    public class ClickStatistics
    {
        public int TotalClicks { get; set; } = 0;
        public int SuccessfulClicks { get; set; } = 0;
        public int FailedClicks { get; set; } = 0;
        public TimeSpan TotalRuntime { get; set; } = TimeSpan.Zero;
        public double AverageClicksPerMinute { get; set; } = 0;
        public DateTime LastClickTime { get; set; } = DateTime.MinValue;
        public DateTime SessionStartTime { get; set; } = DateTime.Now;
        public Dictionary<string, int> ClicksByPosition { get; set; } = new Dictionary<string, int>();
        public Dictionary<ClickType, int> ClicksByType { get; set; } = new Dictionary<ClickType, int>();
    }

    public enum TriggerType
    {
        Manual,
        Scheduled,
        ColorDetection,
        ImageRecognition,
        WindowAppears,
        WindowDisappears,
        PixelChange,
        Hotkey
    }

    public class TriggerSettings
    {
        public TriggerType TriggerType { get; set; } = TriggerType.Manual;
        public bool IsEnabled { get; set; } = false;
        public string? TriggerParameters { get; set; }
        public int TriggerDelay { get; set; } = 0;
        public bool RepeatTrigger { get; set; } = false;
        public int MaxTriggers { get; set; } = 0;
    }

    public class SafetySettings
    {
        public bool EnableSafetyLimits { get; set; } = true;
        public int MaxClicksPerMinute { get; set; } = 600;
        public int MaxRuntimeMinutes { get; set; } = 60;
        public bool EnableEmergencyStop { get; set; } = true;
        public string EmergencyStopHotkey { get; set; } = "F12";
        public bool EnablePauseOnMouseMove { get; set; } = false;
        public int MouseMovementThreshold { get; set; } = 50;
        public bool EnableScreenChangeDetection { get; set; } = false;
        public bool StopOnScreenChange { get; set; } = true;
        public bool EnableAntiDetectionMode { get; set; } = false;
        public double RandomizationFactor { get; set; } = 0.3;
        public int MinBreakInterval { get; set; } = 30;
        public int MaxBreakInterval { get; set; } = 120;
        public int MinBreakDuration { get; set; } = 2000;
        public int MaxBreakDuration { get; set; } = 10000;
    }
}
