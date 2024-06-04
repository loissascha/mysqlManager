using System.Diagnostics;

namespace MySqlManager.Services;

public abstract class MySqlDumpService
{
    public static bool IsCommandAvailable()
    {
        using var process = new Process();
        process.StartInfo.FileName = "mysqldump";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.Arguments = "--version";
         
        try
        {
            process.Start();
            process.WaitForExit();
        }
        catch (System.ComponentModel.Win32Exception) // this gets caught by Windows machines
        {
            return false;
        }
        
        return process.ExitCode == 0; // this checks if it's available on unix machines
    }
    
    public static void DumpDatabase(string host, string port, string user, string password, string database)
    {
        var settingsPath = SettingsService.GetSettingsFolderPath();
        var filepath = Path.Combine(settingsPath, "dump_" + database + ".sql");

        if (File.Exists(filepath))
        {
            File.Delete(filepath);
        }
        
        using Process process = new Process();
        process.StartInfo.FileName = "mysqldump";
        process.StartInfo.RedirectStandardInput = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.Arguments = $"--skip-ssl --host={host} --user={user} --password={password} --port={port} {database}";
        process.StartInfo.UseShellExecute = false;

        process.Start();

        using (StreamReader reader = process.StandardOutput)
        {
            string result = reader.ReadToEnd();

            File.WriteAllText(filepath, result);
        }

        process.WaitForExit();
    }
    
    public static void DumpTable(string host, string port, string user, string password, string database, string table)
    {
        var settingsPath = SettingsService.GetSettingsFolderPath();
        var filepath = Path.Combine(settingsPath, "dump_" + database + "_" + table + ".sql");
        
        using Process process = new Process();
        process.StartInfo.FileName = "mysqldump";
        process.StartInfo.RedirectStandardInput = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.Arguments = $"--host={host} --user={user} --password={password} --port={port} {database} {table}";
        process.StartInfo.UseShellExecute = false;

        process.Start();

        using (StreamReader reader = process.StandardOutput)
        {
            string result = reader.ReadToEnd();

            File.WriteAllText(filepath, result);
        }

        process.WaitForExit();
    }
    
    public static void MySqlImport(string host, string port, string user, string password, string database, string filepath)
    {
        using Process process = new Process();
        process.StartInfo.FileName = "mysql";
        process.StartInfo.RedirectStandardInput = true; 
        process.StartInfo.RedirectStandardOutput = false;
        process.StartInfo.Arguments = $"--host={host} --user={user} --password={password} --port={port} {database}";
        process.StartInfo.UseShellExecute = false;

        process.Start();

        using (StreamWriter writer = process.StandardInput)
        {
            string fileContent = File.ReadAllText(filepath);
            writer.Write(fileContent);
        }

        process.WaitForExit();
    }
}