using MySqlConnector;

namespace MySqlManager.Services;

public class TruncateService(DatabaseConnectionService _databaseConnectionService)
{
    public async Task TruncateTable(string database, string table)
    {
        await using var conn = await _databaseConnectionService.EstablishConnection();
        
        await using var cmd = new MySqlCommand($"USE {database};SET FOREIGN_KEY_CHECKS = 0;TRUNCATE {table};SET FOREIGN_KEY_CHECKS = 1;", conn);
        await cmd.ExecuteNonQueryAsync();
        /*
         * USE databaseName;
         * SET FOREIGN_KEY_CHECKS = 0;
         * TRUNCATE tableName;
         * SET FOREIGN_KEY_CHECKS = 1;
         */
    }
}