namespace server.Configuration;

public class CassandraSettings
{
    public string[] ContactPoints { get; set; } = ["localhost:9042"];
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
} 