using System.Data;
using System.Text.RegularExpressions;
using MySqlConnector;
using MySqlManager.Dtos;

namespace MySqlManager.Services;

public class MySqlManagerService
{
    private const string ConnectionString = "server=localhost;port=30306;user=root;password=root";
    
    private static async Task<MySqlConnection> EstablishConnection()
    {
        var conn = new MySqlConnection(ConnectionString);
        await conn.OpenAsync();
        return conn;
    }

    public async Task<TableData> GetTableContents(string? databaseName, string? tableName, int offset = 0, int limit = 0)
    {
        if (string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(tableName))
        {
            throw new ArgumentException("Database name and table name must be provided");
        }
        
        var tableColumnInformation = await GetTableColumnInfos(databaseName, tableName);
        
        var result = new TableData
        {
            ColumnInformation = tableColumnInformation,
            Content = new List<List<string?>>()
        };
        
        var fieldsCount = tableColumnInformation.Count;
        
        var limits = limit > 0 ? $"LIMIT {offset}, {limit}" : "";
        
        await using var conn = await EstablishConnection();
        await using var cmd = new MySqlCommand($"USE {databaseName}; SELECT * FROM {tableName} {limits}", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new List<string?>();
            for (var i = 0; i < fieldsCount; i++)
            {
                var columnValue = reader.GetValue(i).ToString();
                row.Add(columnValue);
                //Console.WriteLine($"{tableColumnInformation[i].Field}: {columnValue}");
            }
            result.Content.Add(row);
        }

        return result;
    }

    private async Task<List<TableColumnInformation>> GetTableColumnInfos(string? databaseName, string? tableName)
    {
        if (string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(tableName))
        {
            throw new ArgumentException("Database name and table name must be provided");
        }
        
        var result = new List<TableColumnInformation>();
        
        await using var conn = await EstablishConnection();
        
        await using var cmd = new MySqlCommand($"USE {databaseName}; SHOW COLUMNS FROM {tableName}", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var fieldValue = reader.GetValue(0).ToString();
            var typeValue = reader.GetValue(1).ToString();
            var nullValue = reader.GetValue(2).ToString();
            var keyValue = reader.GetValue(3).ToString();
            var defaultValue = reader.GetValue(4).ToString();
            var extraValue = reader.GetValue(5).ToString();
            result.Add(new TableColumnInformation
            {
                Field = fieldValue,
                Type = typeValue,
                Null = nullValue,
                Key = keyValue,
                Default = defaultValue,
                Extra = extraValue
            });
            Console.WriteLine($"{fieldValue} {typeValue} {nullValue} {keyValue} {defaultValue} {extraValue}");
        }
        await reader.CloseAsync();

        // get references to other tables 
        await using var cmd2 = new MySqlCommand($"SELECT TABLE_NAME, COLUMN_NAME, CONSTRAINT_NAME, REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE REFERENCED_TABLE_SCHEMA = '{databaseName}' AND TABLE_NAME = '{tableName}';", conn);
        await using var reader2 = await cmd2.ExecuteReaderAsync();
        while (await reader2.ReadAsync())
        {
            var columnName = reader.GetValue(1).ToString();
            var constraintName = reader.GetValue(2).ToString();
            var referencedTableName = reader.GetValue(3).ToString();
            var referencedColumnName = reader.GetValue(4).ToString();

            foreach (var x in result)
            {
                if (x.Field != columnName)
                    continue;
                x.ReferencedTableName = referencedTableName;
                x.ReferencedColumnName = referencedColumnName;
                Console.WriteLine($"Reference: {columnName} -> {referencedTableName}/{referencedColumnName}");
            }
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

    public async Task<RunSqlResult> RunSql(string? database, string? sql)
    {
        if (string.IsNullOrEmpty(sql))
        {
            throw new ArgumentException("SQL must be provided.");
        }

        var result = new RunSqlResult();
        
        await using var conn = await EstablishConnection();
        
        // if its a select query -> get the actual count for pagination
        var resultCount = 0;
        if (sql.ToLower().Contains("select"))
        {
            try
            {
                var countSql = ConvertToCountQuery(sql);
                await using var countCmd = new MySqlCommand($"USE {database};{countSql}", conn);
                resultCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
            }
            catch (ArgumentException)
            {
                resultCount = -1;
            }
        }
        result.ResultCount = resultCount;

        if (!sql.ToLower().Contains("limit"))
        {
            sql += " LIMIT 0, 300";
            result.Offset = 0;
            result.Limit = 300;
        }
        
        Console.WriteLine($"RunSQL: {sql}");
        
        await using var cmd = new MySqlCommand($"USE {database};{sql}", conn);
        var dataTable = new DataTable();
        await using var dataReader = await cmd.ExecuteReaderAsync();
        dataTable.Load(dataReader);
        await dataReader.CloseAsync();

        result.DataTable = dataTable;

        return result;
    }
    
    private static string ConvertToCountQuery(string originalQuery)
    {
        var match = Regex.Match(originalQuery, @"SELECT\s+.*\s+FROM", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var selectPart = match.Value;
            var countQuery = originalQuery.Replace(selectPart, "SELECT COUNT(*) FROM");
            return countQuery;
        }
        else
        {
            throw new ArgumentException("SQL does not contain a valid SELECT ... FROM clause.");
        }
    }
}

