using MySqlConnector;

namespace MySqlManager.Services;

public class MySqlManagerService
{
    private static async Task<MySqlConnection> EstablishConnection()
    {
        var conn = new MySqlConnection("server=localhost;port=30306;user=root;password=root");
        await conn.OpenAsync();
        return conn;
    }
    
    public async Task<ServerVersions> GetServerVersion()
    {
        var result = new ServerVersions();

        await Task.Delay(500);

        await using var conn = await EstablishConnection();

        //await using var cmd = new MySqlCommand("SELECT VERSION()", conn);
        await using var cmd = new MySqlCommand("SHOW VARIABLES LIKE '%version%'", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var variableName = reader.GetString(0);
            var variableValue = reader.GetString(1);

            
            if (variableName == "version")
            {
                result.Version = variableValue;
                if (result.Version.Contains("MariaDB"))
                {
                    result.ServerType = "MariaDB";
                }
            }
            else if (variableName == "protocol_version")
            {
                result.ProtocolVersion = variableValue;
            }
            else if (variableName == "version_compile_os")
            {
                result.VersionCompileOs = variableValue;
            }
            
            Console.WriteLine(variableName + ": " + variableValue);
            //return value;
        }

        return result;
    }
}

public class ServerVersions
{
    public string? ServerType { get; set; }
    public string? Version { get; set; }
    public string? ProtocolVersion { get; set; }
    public string? VersionCompileOs { get; set; }
}