namespace MySqlManager.Dtos;

public class NewTableRow
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Length { get; set; }
    public string? Default { get; set; }
    public bool Nullable { get; set; }
    public bool AutoIncrement { get; set; }
    public bool PrimaryKey { get; set; }
}