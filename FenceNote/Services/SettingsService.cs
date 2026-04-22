using System;
using System.IO;
using System.Text.Json;

namespace FenceNote.Services
{
    public class AppSettings
    {
        public bool IsDarkMode { get; set; }
    }

    public class SettingsService
    {
        private readonly string _settingsPath;

        public SettingsService()
        {
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(folder, "FenceNote");
            Directory.CreateDirectory(appFolder);
            _settingsPath = Path.Combine(appFolder, "settings.json");
        }

        public AppSettings LoadSettings()
        {
            if (!File.Exists(_settingsPath))
            {
                return new AppSettings { IsDarkMode = false };
            }

            try
            {
                string json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings { IsDarkMode = false };
            }
        }

        public void SaveSettings(AppSettings settings)
        {
            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(_settingsPath, json);
        }
    }
}