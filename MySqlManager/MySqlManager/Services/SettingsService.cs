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
                ConnectionStrings = new List<ConnectionSetting>()
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
        
        foreach (var connectionString in Settings!.ConnectionStrings)
        {
            connectionString.Active = false;
        }
        
        Settings!.ConnectionStrings.Add(new ConnectionSetting
        {
            // ConStr = $"server={host};port={port};user={user};password={password};",
            Active = true,
            Server = host,
            Port = port,
            User = user,
            Password = password
        });
        SaveSettings();
    }

    public ConnectionSetting? GetActiveConnectionSetting()
    {
        foreach (var connectionSetting in Settings!.ConnectionStrings)
        {
            if (connectionSetting.Active)
            {
                return connectionSetting;
            }
        }

        return null;
    }

    public string GetActiveConnectionString()
    {
        var activeConnectionString = Settings!.ConnectionStrings.FirstOrDefault(x => x.Active);
        if (activeConnectionString == null) return "";
        var conStr = $"server={activeConnectionString.Server};port={activeConnectionString.Port};user={activeConnectionString.User};password={activeConnectionString.Password};";
        return conStr;
    }

    public int GetActiveConnectionStringIndex()
    {
        for (var i = 0; i < Settings!.ConnectionStrings.Count; i++)
        {
            if (Settings!.ConnectionStrings[i].Active)
                return i;
        }
        return 0;
    }

    public void SetConnectionIndexActive(int index)
    {
        Console.WriteLine($"Active Connection string 1 : {GetActiveConnectionString()} new one will be index: {index}");
        foreach (var connectionString in Settings!.ConnectionStrings)
        {
            connectionString.Active = false;
        }
        Console.WriteLine($"Removed active flag. Now: {GetActiveConnectionString()}");
        Settings!.ConnectionStrings[index].Active = true;
        Console.WriteLine("SetConnectionIndexActive. New Active connection string: " + GetActiveConnectionString());
        SaveSettings();
    }
}

public class Settings
{
    public required List<ConnectionSetting> ConnectionStrings { get; set; }
}

public class ConnectionSetting
{
    public required string Server { get; set; }
    public string? Port { get; set; }
    public required string User { get; set; }
    public required string Password { get; set; }
    public bool Active { get; set; }
}