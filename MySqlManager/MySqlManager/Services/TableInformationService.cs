using System.Text;
using MySqlConnector;
using MySqlManager.Dtos;

namespace MySqlManager.Services;

public class TableInformationService(DatabaseConnectionService _databaseConnectionService)
{
    public async Task<List<TableColumnInformation>> GetTableColumnInfos(string? databaseName, string? tableName)
    {
        if (string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(tableName))
        {
            throw new ArgumentException("Database name and table name must be provided");
        }
        
        var result = new List<TableColumnInformation>();
        
        await using var conn = await _databaseConnectionService.EstablishConnection();
        
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

        await using var conn = await _databaseConnectionService.EstablishConnection();
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
        
        await using var conn = await _databaseConnectionService.EstablishConnection();
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

    
}