using System;
using System.IO;
using AutoClicker.Models;
using Newtonsoft.Json;

namespace AutoClicker.Services
{
    public static class ConfigService
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AutoClickerPro",
            "config.json"
        );

        static ConfigService()
        {
            var directory = Path.GetDirectoryName(ConfigPath);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static void SaveSettings(ClickSettings settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save settings: {ex.Message}", ex);
            }
        }

        public static ClickSettings LoadSettings()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var settings = JsonConvert.DeserializeObject<ClickSettings>(json);
                    return settings ?? new ClickSettings();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load settings: {ex.Message}", ex);
            }

            return new ClickSettings();
        }

        public static void ExportSettings(ClickSettings settings, string filePath)
        {
            try
            {
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to export settings: {ex.Message}", ex);
            }
        }

        public static ClickSettings ImportSettings(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var settings = JsonConvert.DeserializeObject<ClickSettings>(json);
                return settings ?? new ClickSettings();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to import settings: {ex.Message}", ex);
            }
        }

        public static void ResetToDefaults()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    File.Delete(ConfigPath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to reset settings: {ex.Message}", ex);
            }
        }
    }
}
