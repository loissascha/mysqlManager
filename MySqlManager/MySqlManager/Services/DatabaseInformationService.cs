using MySqlConnector;
using MySqlManager.Dtos;

namespace MySqlManager.Services;

public class DatabaseInformationService(DatabaseConnectionService _databaseConnectionService, TableInformationService _tableInformationService)
{
    public List<DatabaseInformation> DatabaseList { get; set; } = new();

    public event Action? OnDatabaseListChanged;

    public void ResetDatabaseList()
    {
        DatabaseList = new List<DatabaseInformation>();
        OnDatabaseListChanged?.Invoke();
    }

    public async Task RefreshDatabaseList()
    {
        if (await _databaseConnectionService.IsConnectionPossible())
        {
            Console.WriteLine("MySqlManagerService Connection possible. Getting database list...");
            DatabaseList = await GetDatabaseList(false);
        }
        else
        {
            DatabaseList = new List<DatabaseInformation>();
        }
        Console.WriteLine("Invoke Database List Changed.");
        OnDatabaseListChanged?.Invoke();
    }
    
    public async Task<List<DatabaseInformation>> GetDatabaseList(bool includeTableInformation = true)
    {
        var result = new List<DatabaseInformation>();
        
        await using var conn = await _databaseConnectionService.EstablishConnection();
        await using var cmd = new MySqlCommand("SHOW DATABASES", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var databaseName = reader.GetString(0);
            var dbInfo = new DatabaseInformation
            {
                Name = databaseName,
                Tables = await _tableInformationService.GetTableList(databaseName, includeTableInformation)
            };
            result.Add(dbInfo);
        }

        return result;
    }
    
    public async Task<ServerInformationDto> GetServerVersion()
    {
        var result = new ServerInformationDto();

        await Task.Delay(500);

        await using var conn = await _databaseConnectionService.EstablishConnection();

        await using var cmd = new MySqlCommand("SHOW VARIABLES LIKE '%version%'", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var variableName = reader.GetString(0);
            var variableValue = reader.GetString(1);
            
            switch (variableName)
            {
                case "version":
                {
                    result.Version = variableValue;
                    if (result.Version.Contains("MariaDB"))
                    {
                        result.ServerType = "MariaDB";
                    }

                    break;
                }
                case "version_comment":
                {
                    if (variableValue.ToLower().Contains("mysql community server") && string.IsNullOrEmpty(result.ServerType))
                    {
                        result.ServerType = variableValue;
                    }

                    break;
                }
                case "protocol_version":
                    result.ProtocolVersion = variableValue;
                    break;
                case "version_compile_os":
                    result.VersionCompileOs = variableValue;
                    break;
            }
            
            Console.WriteLine(variableName + ": " + variableValue);
            //return value;
        }

        return result;
    }
    
    public async Task<List<string>> GetCollations()
    {
        await using var conn = await _databaseConnectionService.EstablishConnection();
        await using var cmd = new MySqlCommand("SHOW COLLATION", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        var collations = new List<string>();
        while (await reader.ReadAsync())
        {
            Console.WriteLine($"{reader.GetString(0)}");
            collations.Add(reader.GetString(0));
        }

        return collations;
    }
}