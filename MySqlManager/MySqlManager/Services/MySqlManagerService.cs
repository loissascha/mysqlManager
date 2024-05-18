using MySqlConnector;

namespace MySqlManager.Services;

public class MySqlManagerService(
    DatabaseConnectionService _databaseConnectionService,
    DatabaseInformationService _databaseInformationService)
{
    //private const string ConnectionString = "server=localhost;port=30306;user=root;password=root";

    public async void Init()
    {
        Console.WriteLine("MySqlManagerService Init...");
        if (await _databaseConnectionService.IsConnectionPossible())
        {
            await _databaseInformationService.RefreshDatabaseList();
        }
        Console.WriteLine("MySqlManagerService Init done.");
    }

    public async Task CreateDatabase(string name, string collation)
    {
        // character set = first part of collation
        var collationSplit = collation.Split("_");
        var charset = collationSplit[0];
        var createDatabaseQuery = $"CREATE DATABASE `{name}` CHARACTER SET `{charset}` COLLATE `{collation}`;";
        
        await using var conn = await _databaseConnectionService.EstablishConnection();
        await using var cmd = new MySqlCommand(createDatabaseQuery, conn);

        await cmd.ExecuteNonQueryAsync();
    }
}

