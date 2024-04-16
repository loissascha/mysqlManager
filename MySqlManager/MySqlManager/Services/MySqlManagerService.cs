using MySqlConnector;
using MySqlManager.Dtos;

namespace MySqlManager.Services;

public class MySqlManagerService
{
    private static async Task<MySqlConnection> EstablishConnection()
    {
        var conn = new MySqlConnection("server=localhost;port=30306;user=root;password=root");
        await conn.OpenAsync();
        return conn;
    }
    
    public async Task<ServerInformationDto> GetServerVersion()
    {
        var result = new ServerInformationDto();

        await Task.Delay(500);

        await using var conn = await EstablishConnection();

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
}

