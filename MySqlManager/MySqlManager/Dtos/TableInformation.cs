namespace MySqlManager.Dtos;

public class TableInformation
{
    public string? Name { get; set; }
    public string? Collation { get; set; }
    public string? Engine { get; set; }
    public string? Rows { get; set; }
    public decimal? SizeInMb { get; set; }
}