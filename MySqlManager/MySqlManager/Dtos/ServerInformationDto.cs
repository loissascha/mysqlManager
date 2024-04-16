namespace MySqlManager.Dtos;

public class ServerInformationDto
{
    public string? ServerType { get; set; }
    public string? Version { get; set; }
    public string? ProtocolVersion { get; set; }
    public string? VersionCompileOs { get; set; }
}