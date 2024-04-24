using Newtonsoft.Json;

namespace MySqlManager.Services;

public class SettingsService
{
    public Settings? Settings { get; private set; }
    
    public void Init()
    {
        Console.WriteLine("Settings Service init");
        var settingsFolderPath = GetSettingsFolderPath();
        if (!Directory.Exists(settingsFolderPath))
        {
            Directory.CreateDirectory(settingsFolderPath);
            var defaultSettings = new Settings()
            {
                ConnectionString = ""
            };
            var defaultSettingsJson = JsonConvert.SerializeObject(defaultSettings);
            var settingsFilePath = Path.Combine(settingsFolderPath, "settings.json");
            File.WriteAllText(settingsFilePath, defaultSettingsJson);
        }
        LoadSettings();
    }

    private string GetSettingsFolderPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "loisMySqlManager");
    }

    private void LoadSettings()
    {
        var settingsFilePath = Path.Combine(GetSettingsFolderPath(), "settings.json");
        if (File.Exists(settingsFilePath))
        {
            var settingsJson = File.ReadAllText(settingsFilePath);
            Settings = JsonConvert.DeserializeObject<Settings>(settingsJson);
            if (Settings == null)
                throw new Exception("Settings file is corrupted!");
        }
        else
            throw new Exception("Settings file does not exist!");
    }
}

public class Settings
{
    public required string ConnectionString { get; set; }
}