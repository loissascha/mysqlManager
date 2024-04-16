namespace MySqlManager.Dtos;

public class TableColumnInformation
{
    public required string? Field { get; set; }
    public string? Type { get; set; }
    public string? Null { get; set; }
    public string? Key { get; set; }
    public string? Default { get; set; }
    public string? Extra { get; set; }
    public string? ReferencedTableName { get; set; }
    public string? ReferencedColumnName { get; set; }
}