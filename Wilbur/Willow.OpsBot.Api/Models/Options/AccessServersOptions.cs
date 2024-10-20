namespace Willow.OpsBot.Api.Models.Options;

public class AccessServersOptions
{
    public string[] SqlServer { get; set; } = null!;
    public string[] PostgreSql { get; set; } = null!;
    public const string Position = "AccessServers";
}
