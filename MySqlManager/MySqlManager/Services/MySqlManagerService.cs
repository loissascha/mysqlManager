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
}

