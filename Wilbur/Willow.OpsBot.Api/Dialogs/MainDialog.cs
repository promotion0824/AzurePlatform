using Microsoft.Bot.Builder.Dialogs;
using Willow.OpsBot.Common.Models.Enums;

namespace Willow.OpsBot.Api.Dialogs;

public class MainDialog : LogoutDialog
{
    private readonly List<string> _firewallPhrases = new() { "fw", "firewall", "sql", "pg", "allow ip", "allow my ip" };
    private readonly ILogger _logger;

    public MainDialog(AccessDialog accessDialog, ILogger<MainDialog> logger, IConfiguration configuration) : base(nameof(MainDialog), configuration["ConnectionName"])
    {
        _logger = logger;

        AddDialog(accessDialog);
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
        {
            StartAccessFlow
        }));

        InitialDialogId = nameof(WaterfallDialog);
    }


    private async Task<DialogTurnResult> StartAccessFlow(WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        if (stepContext.Context?.Activity?.Text != null && _firewallPhrases.Any(p =>
                stepContext.Context.Activity.Text.ToLowerInvariant().Trim().StartsWith(p)))
        {
            _logger.LogDebug("Pre filling access prompt with a type of firewall from {Msg}",
                stepContext.Context.Activity.Text);
            return await stepContext.BeginDialogAsync(nameof(AccessDialog),
                new AccessDetails { Type = AccessTypes.Firewall }, cancellationToken);
        }

        return await stepContext.BeginDialogAsync(nameof(AccessDialog), new AccessDetails(), cancellationToken);
    }
}
