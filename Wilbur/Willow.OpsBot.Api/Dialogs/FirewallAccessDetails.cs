using Willow.OpsBot.Common.Models.Enums;

namespace Willow.OpsBot.Api.Dialogs;

public record FirewallAccessDetails
{
    public string? Server { get; set; }

    public string? Ip { get; set; }
    public ServerTypes Type { get; set; }
    public bool Prefilled { get; set; }
}
