namespace MySqlManager.Dtos;

public class TableData
{
    public required List<TableColumnInformation> ColumnInformation { get; set; }
    public required List<List<string?>> Content { get; set; }
}