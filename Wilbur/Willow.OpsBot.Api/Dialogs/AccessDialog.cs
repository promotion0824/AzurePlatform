using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using Willow.OpsBot.Api.Models;
using Willow.OpsBot.Common.Models.Enums;


namespace Willow.OpsBot.Api.Dialogs;

public class AccessDialog : LogoutDialog
{
    private readonly ILogger _logger;

    public AccessDialog(FirewallAccessDialog firewallAccessDialog, ILogger<AccessDialog> logger, IConfiguration configuration) : base(
        nameof(AccessDialog), configuration["ConnectionName"])
    {
        _logger = logger;

        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
        AddDialog(firewallAccessDialog);
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
        {
            AccessType,
            StartAccessFlow
        }));

        InitialDialogId = nameof(WaterfallDialog);
    }

    private async Task<DialogTurnResult> AccessType(WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        var accessDetails = (AccessDetails?)stepContext.Options ?? new AccessDetails();

        if (accessDetails.Type == 0)
        {
            _logger.LogDebug("Asking for access type");

            var choices = Enum.GetNames<AccessTypes>();


            var content = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock("What type of access do you need?")
                },
                Actions = choices.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Style = "positive",
                    Type = AdaptiveSubmitAction.TypeName,
                    Data = new AdaptiveCardAction
                    {
                        MsteamsCardAction = new CardAction
                        {
                            Type = ActionTypes.MessageBack,
                            DisplayText = choice,
                            Text = choice
                        }
                    }
                }).ToList<AdaptiveAction>()
            };
            var attachment = MessageFactory.Attachment(new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JObject.FromObject(content)
            });
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)attachment,
                Choices = ChoiceFactory.ToChoices(choices),
                Style = ListStyle.None
            }, cancellationToken);
        }

        return await stepContext.NextAsync(accessDetails.Type, cancellationToken);
    }

    private async Task<DialogTurnResult> StartAccessFlow(WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        var serverAccessDetails = (AccessDetails?)stepContext.Options ?? new AccessDetails();
        serverAccessDetails.Type = stepContext.Result switch
        {
            FoundChoice t => Enum.Parse<AccessTypes>(t.Value ?? string.Empty),
            AccessTypes t => t,
            _ => throw new ArgumentOutOfRangeException(nameof(stepContext), stepContext, null)
        };

        return serverAccessDetails.Type switch
        {
            AccessTypes.Firewall => await stepContext.BeginDialogAsync(nameof(FirewallAccessDialog),
                new FirewallAccessDetails(), cancellationToken),
            _ => await stepContext.EndDialogAsync(cancellationToken: cancellationToken)
        };
    }
}
