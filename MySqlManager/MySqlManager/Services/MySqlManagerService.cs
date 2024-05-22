using System.Text;
using MySqlConnector;
using MySqlManager.Dtos;

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
    
    public async Task CreateTable(string database, string tableName, List<NewTableRow> tableRows)
    {
        await using var conn = await _databaseConnectionService.EstablishConnection();
        var sb = new StringBuilder($"CREATE TABLE {database}.{tableName} (");
        var primaryKey = "";

        foreach (var row in tableRows)
        {
            var columnDef = $"{row.Name} {row.Type}";
            if (!string.IsNullOrEmpty(row.Length?.Trim()))
            {
                columnDef += $"({row.Length})";
            }
            
            if (!row.Nullable)
            {
                columnDef += " NOT NULL";
            }
            
            if(row.AutoIncrement)
            {
                columnDef += " AUTO_INCREMENT";
            }

            if (!string.IsNullOrEmpty(row.Default?.Trim()))
            {
                columnDef += $" DEFAULT '{row.Default}'";
            }

            sb.Append(columnDef + ",");
            
            if (row.PrimaryKey)
            {
                primaryKey += $"{row.Name},";
            }
        }
        
        if (!string.IsNullOrEmpty(primaryKey))
        {
            // removes trailing comma
            primaryKey = primaryKey.Remove(primaryKey.Length - 1);
            sb.Append($" PRIMARY KEY ({primaryKey})");
        }
        else // removes trailing comma
        {
            sb.Length--;
        }

        sb.Append(");");
        
        Console.WriteLine(sb.ToString());

        await using var cmd = new MySqlCommand(sb.ToString(), conn);
        await cmd.ExecuteNonQueryAsync();
    }
}

