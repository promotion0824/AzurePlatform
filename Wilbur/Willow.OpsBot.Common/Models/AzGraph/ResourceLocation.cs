// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Willow.OpsBot.Common.Models.AzGraph;

public class ResourceLocation
{
    public string? Name { get; init; }
    public string? Type { get; init; }
    public string? SubscriptionId { get; init; }
    public string? ResourceGroup { get; init; }
}
