using System.Diagnostics;

namespace MySqlManager.Services;

public abstract class MySqlDumpService
{
    public static bool IsCommandAvailable()
    {
        using var process = new Process();
        process.StartInfo.FileName = "mariadb-dump";
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
        
        using var process = new Process();
        process.StartInfo.FileName = "mariadb-dump";
        process.StartInfo.RedirectStandardInput = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.Arguments = $"--skip-ssl --host={host} --user={user} --password={password} --port={port} {database}";
        process.StartInfo.UseShellExecute = false;

        process.Start();

        using (var reader = process.StandardOutput)
        {
            var result = reader.ReadToEnd();

            File.WriteAllText(filepath, result);
        }

        process.WaitForExit();
    }
    
    public static void DumpTable(string host, string port, string user, string password, string database, string table)
    {
        var settingsPath = SettingsService.GetSettingsFolderPath();
        var filepath = Path.Combine(settingsPath, "dump_" + database + "_" + table + ".sql");
        
        using var process = new Process();
        process.StartInfo.FileName = "mariadb-dump";
        process.StartInfo.RedirectStandardInput = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.Arguments = $"--skip-ssl --host={host} --user={user} --password={password} --port={port} {database} {table}";
        process.StartInfo.UseShellExecute = false;

        process.Start();

        using (var reader = process.StandardOutput)
        {
            var result = reader.ReadToEnd();

            File.WriteAllText(filepath, result);
        }

        process.WaitForExit();
    }
    
    public static void MySqlImport(string host, string port, string user, string password, string database, string filepath)
    {
        using var process = new Process();
        process.StartInfo.FileName = "mysql";
        process.StartInfo.RedirectStandardInput = true; 
        process.StartInfo.RedirectStandardOutput = false;
        process.StartInfo.Arguments = $"--host={host} --user={user} --password={password} --port={port} {database}";
        process.StartInfo.UseShellExecute = false;

        process.Start();

        using (var writer = process.StandardInput)
        {
            var fileContent = File.ReadAllText(filepath);
            writer.Write(fileContent);
        }

        process.WaitForExit();
    }
}