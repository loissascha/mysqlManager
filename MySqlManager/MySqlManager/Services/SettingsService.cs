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
            
        }
        var settingsFilePath = Path.Combine(settingsFolderPath, "settings.json");
        if (!File.Exists(settingsFilePath))
        {
            var defaultSettings = new Settings()
            {
                ConnectionStrings = new List<ConnectionString>()
            };
            var defaultSettingsJson = JsonConvert.SerializeObject(defaultSettings);
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

    private void SaveSettings()
    {
        var settingsFilePath = Path.Combine(GetSettingsFolderPath(), "settings.json");
        var settingsJson = JsonConvert.SerializeObject(Settings);
        File.WriteAllText(settingsFilePath, settingsJson);
    }

    public void AddConnectionString(string host, string port, string user, string password)
    {
        if (Settings == null)
            throw new Exception("Settings file is corrupted!");
        
        Console.WriteLine("Setting Connection String and saving Settings...");
        
        Settings!.ConnectionStrings.ForEach(x => x.IsActive = false);
        Settings!.ConnectionStrings.Add(new ConnectionString
        {
            ConStr = $"server={host};port={port};user={user};password={password};",
            IsActive = true
        });
        SaveSettings();
    }

    public string GetActiveConnectionString()
    {
        return Settings!.ConnectionStrings.Where(x => x.IsActive).Select(x => x.ConStr).FirstOrDefault() ?? "";
    }

    public void SetConnectionIndexActive(int index)
    {
        Settings!.ConnectionStrings.ForEach(x => x.IsActive = false);
        Settings!.ConnectionStrings[index].IsActive = true;
        SaveSettings();
    }
}

public class Settings
{
    public required List<ConnectionString> ConnectionStrings { get; set; }
}

public class ConnectionString
{
    public required string ConStr { get; set; }
    public bool IsActive { get; set; }
}