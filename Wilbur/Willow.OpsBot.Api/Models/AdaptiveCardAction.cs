using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace Willow.OpsBot.Api.Models;

public class AdaptiveCardAction
{
    [JsonProperty("msteams")] public CardAction? MsteamsCardAction { get; set; }

    [JsonProperty("command")] public string? Command { get; set; }

    [JsonProperty("postedValues")] public string? PostedValues { get; set; }

    [JsonProperty("cardId")] public string? CardId { get; set; }

    [JsonProperty("teamId")] public string? TeamId { get; set; }

    [JsonProperty("ticketId")] public string? TicketId { get; set; }

    [JsonProperty("activityId")] public string? ActivityId { get; set; }
}
