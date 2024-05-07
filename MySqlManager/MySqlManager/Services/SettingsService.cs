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

    public static string GetSettingsFolderPath()
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
        Console.WriteLine("Saving Settings...");
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

    public int GetActiveConnectionStringIndex()
    {
        for (var i = 0; i < Settings!.ConnectionStrings.Count; i++)
        {
            if (Settings!.ConnectionStrings[i].IsActive)
                return i;
        }
        return 0;
    }

    public void SetConnectionIndexActive(int index)
    {
        Console.WriteLine($"Active Connection string 1 : {GetActiveConnectionString()} new one will be index: {index}");
        foreach (var connectionString in Settings!.ConnectionStrings)
        {
            connectionString.IsActive = false;
        }
        Console.WriteLine($"Removed active flag. Now: {GetActiveConnectionString()}");
        Settings!.ConnectionStrings[index].IsActive = true;
        Console.WriteLine("SetConnectionIndexActive. New Active connection string: " + GetActiveConnectionString());
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