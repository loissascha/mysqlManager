using System.Data;
using System.Text.RegularExpressions;
using MySqlConnector;
using MySqlManager.Dtos;

namespace MySqlManager.Services;

public class SqlCommandService(DatabaseConnectionService _databaseConnectionService)
{
    public async Task<RunSqlResult> RunSql(string? database, string? sql)
    {
        if (string.IsNullOrEmpty(sql))
        {
            throw new ArgumentException("SQL must be provided.");
        }

        // remove ending ';' if there is one (fixes issue with limit if it's not manually set)
        sql = sql.Trim();
        while (sql.EndsWith(';'))
        {
            sql = sql.TrimEnd(';');
        }

        var result = new RunSqlResult();
        
        
        await using var conn = await _databaseConnectionService.EstablishConnection();
        
        // if its a select query -> get the actual count for pagination
        var resultCount = -1;
        result.Offset = -1;
        result.Limit = -1;
        result.ShowDatagrid = false;
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
            
            // add limit if there is none
            if (!sql.Contains("limit", StringComparison.CurrentCultureIgnoreCase))
            {
                sql += " LIMIT 0, 300";
                result.Offset = 0;
                result.Limit = 300;
            }

            result.ShowDatagrid = true;
        }
        result.ResultCount = resultCount;

        
        Console.WriteLine($"RunSQL: {sql}");
        
        await using var cmd = new MySqlCommand($"USE {database};{sql}", conn);
        var dataTable = new DataTable();
        await using var dataReader = await cmd.ExecuteReaderAsync();
        dataTable.Load(dataReader);
        await dataReader.CloseAsync();

        result.DataTable = dataTable;
        result.ResultCount = dataTable.Rows.Count;

        if (result.Offset == -1)
        {
            result.Offset = 0;
            result.Limit = dataTable.Rows.Count;
        }

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