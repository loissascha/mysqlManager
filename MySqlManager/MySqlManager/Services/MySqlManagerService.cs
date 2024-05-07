using System.Data;
using System.Text.RegularExpressions;
using MySqlConnector;
using MySqlManager.Dtos;

namespace MySqlManager.Services;

public class MySqlManagerService
{
    private readonly SettingsService _settingsService;
    //private const string ConnectionString = "server=localhost;port=30306;user=root;password=root";

    public event Action OnDatabaseListChanged;
    public List<DatabaseInformation> DatabaseList { get; set; }

    public MySqlManagerService(SettingsService settingsService)
    {
        _settingsService = settingsService;
        DatabaseList = new List<DatabaseInformation>();
    }

    public async void Init()
    {
        Console.WriteLine("MySqlManagerService Init...");
        if (await IsConnectionPossible())
        {
            await RefreshDatabaseList();
        }
        Console.WriteLine("MySqlManagerService Init done.");
    }

    public void ResetDatabaseList()
    {
        DatabaseList = new List<DatabaseInformation>();
        OnDatabaseListChanged?.Invoke();
    }

    public async Task RefreshDatabaseList()
    {
        if (await IsConnectionPossible())
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
    
    private async Task<MySqlConnection> EstablishConnection()
    {
        var conn = new MySqlConnection(_settingsService.GetActiveConnectionString());
        await conn.OpenAsync();
        return conn;
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

    public async Task<List<TableInformation>> GetTableList(string databaseName, bool includeTableInformation = true)
    {
        var result = new List<TableInformation>();

        await using var conn = await EstablishConnection();
        await using var cmd = new MySqlCommand($"USE {databaseName}; SHOW TABLES", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var tableName = reader.GetString(0);
            if (includeTableInformation)
            {
                var tableInformation = await GetTableInformation(databaseName, tableName);
                result.Add(tableInformation);
            }
            else
            {
                result.Add(new TableInformation()
                {
                    Name = tableName
                });
            }
        }

        return result;
    }

    private async Task<TableInformation> GetTableInformation(string database, string table)
    {
        var result = new TableInformation();
        result.Name = table;
        
        await using var conn = await EstablishConnection();
        await using var cmd = new MySqlCommand($"USE information_schema;SELECT table_collation, engine, TABLE_ROWS, ROUND(SUM(data_length + index_length) / 1024 / 1024, 2) as `Size in MB` FROM tables WHERE table_schema = '{database}' AND table_name = '{table}'", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Collation = reader.GetValue(0).ToString();
            result.Engine = reader.GetValue(1).ToString();
            result.Rows = reader.GetValue(2).ToString();
            result.SizeInMb = reader.GetDecimal(3);
        }

        return result;
    }

    public async Task<List<DatabaseInformation>> GetDatabaseList(bool includeTableInformation = true)
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
                Tables = await GetTableList(databaseName, includeTableInformation)
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

        // remove ending ';' if there is one (fixes issue with limit if it's not manually set)
        sql = sql.Trim();
        if (sql.EndsWith(';'))
        {
            sql = sql.TrimEnd(';');
        }

        var result = new RunSqlResult();
        
        await using var conn = await EstablishConnection();
        
        // if its a select query -> get the actual count for pagination
        var resultCount = 0;
        if (sql.StartsWith("select", StringComparison.CurrentCultureIgnoreCase))
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

        // add limit if there is none
        if (!sql.Contains("limit", StringComparison.CurrentCultureIgnoreCase))
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

