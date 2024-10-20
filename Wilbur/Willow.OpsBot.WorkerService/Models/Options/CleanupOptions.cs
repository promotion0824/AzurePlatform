namespace Willow.OpsBot.WorkerService.Models.Options;

public class CleanupOptions
{
    public List<string>? ServersExcluded { get; set; }

    public const string Position = "CleanupOptions";
}
