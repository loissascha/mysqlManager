namespace MySqlManager.Dtos;

public class DatabaseInformation
{
    public required string Name { get; set; }
    public required List<string> Tables { get; set; }
}