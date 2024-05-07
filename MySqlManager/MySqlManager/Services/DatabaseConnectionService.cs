using MySqlConnector;

namespace MySqlManager.Services;

public class DatabaseConnectionService(SettingsService _settingsService)
{
    public async Task<bool> IsConnectionPossible()
    {
        try
        {
            await using var conn = await EstablishConnection();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public async Task<MySqlConnection> EstablishConnection()
    {
        var conn = new MySqlConnection(_settingsService.GetActiveConnectionString());
        await conn.OpenAsync();
        return conn;
    }
}