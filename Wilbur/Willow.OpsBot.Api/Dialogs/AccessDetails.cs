using Willow.OpsBot.Common.Models.Enums;

namespace Willow.OpsBot.Api.Dialogs;

public record AccessDetails
{
    public AccessTypes Type { get; set; }
}
