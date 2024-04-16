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

    public async Task<TableData> GetTableContents(string databaseName, string tableName, int offset = 0, int limit = 0)
    {
        var tableColumnInformation = await GetTableColumnInfos(databaseName, tableName);
        
        var result = new TableData
        {
            ColumnInformation = tableColumnInformation,
            Content = new List<Dictionary<int, string?>>()
        };
        
        var fieldsCount = tableColumnInformation.Count;
        
        var limits = limit > 0 ? $"LIMIT {offset}, {limit}" : "";
        
        await using var conn = await EstablishConnection();
        await using var cmd = new MySqlCommand($"USE {databaseName}; SELECT * FROM {tableName} {limits}", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<int, string?>();
            for (var i = 0; i < fieldsCount; i++)
            {
                var columnValue = reader.GetValue(i).ToString();
                row.Add(i, columnValue);
                //Console.WriteLine($"{tableColumnInformation[i].Field}: {columnValue}");
            }
            result.Content.Add(row);
        }

        return result;
    }

    private async Task<List<TableColumnInformation>> GetTableColumnInfos(string databaseName, string tableName)
    {
        var result = new List<TableColumnInformation>();
        
        await using var conn = await EstablishConnection();
        await using var cmd = new MySqlCommand($"USE {databaseName}; SHOW COLUMNS FROM {tableName}", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var fieldValue = reader.GetValue(0).ToString();
            var typeValue = reader.GetValue(0).ToString();
            var nullValue = reader.GetValue(0).ToString();
            var keyValue = reader.GetValue(0).ToString();
            var defaultValue = reader.GetValue(0).ToString();
            var extraValue = reader.GetValue(0).ToString();
            result.Add(new TableColumnInformation
            {
                Field = fieldValue,
                Type = typeValue,
                Null = nullValue,
                Key = keyValue,
                Default = defaultValue,
                Extra = extraValue
            });
        }

        return result;
    }

    private async Task<List<string>> GetTableList(string databaseName)
    {
        var result = new List<string>();

        await using var conn = await EstablishConnection();
        await using var cmd = new MySqlCommand($"USE {databaseName}; SHOW TABLES", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var tableName = reader.GetString(0);
            //Console.WriteLine(tableName);
            result.Add(tableName);
        }

        return result;
    }

    public async Task<List<DatabaseInformation>> GetDatabaseList()
    {
        var result = new List<DatabaseInformation>();
        
        await using var conn = await EstablishConnection();
        await using var cmd = new MySqlCommand("SHOW DATABASES", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var databaseName = reader.GetString(0);
            var dbInfo = new DatabaseInformation
            {
                Name = databaseName,
                Tables = await GetTableList(databaseName)
            };
            result.Add(dbInfo);
        }

        return result;
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
            
            //Console.WriteLine(variableName + ": " + variableValue);
            //return value;
        }

        return result;
    }
}

