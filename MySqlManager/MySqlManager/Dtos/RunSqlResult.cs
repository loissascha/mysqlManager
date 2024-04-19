using System.Data;

namespace MySqlManager.Dtos;

public class RunSqlResult
{
    public DataTable DataTable { get; set;  }
    public int ResultCount { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }
}