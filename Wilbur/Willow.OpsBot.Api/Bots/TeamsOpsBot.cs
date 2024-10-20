using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Willow.OpsBot.Api.Models.Options;

namespace Willow.OpsBot.Api.Bots;

public class TeamsOpsBot<T> : TeamsActivityHandler where T : Dialog
{
    private readonly Dialog _dialog;
    private readonly BotState _conversationState;
    private readonly BotState _userState;
    private readonly ILogger _logger;
    private readonly UrlsOptions _urlsOptions;

    public TeamsOpsBot(ConversationState conversationState, UserState userState, T dialog,
        ILogger<TeamsOpsBot<T>> logger, IOptions<UrlsOptions> urlsOptions)
    {
        _conversationState = conversationState;
        _userState = userState;
        _dialog = dialog;
        _logger = logger;
        _urlsOptions = urlsOptions.Value;
    }

    public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
    {

        await base.OnTurnAsync(turnContext, cancellationToken);

        // Save any state changes that might have occurred during the turn.
        await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
    }


    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running dialog with Message Activity {Text} {@Activity} {@State}", turnContext?.Activity.Text, turnContext?.Activity.Value, turnContext?.TurnState);

        // Run the Dialog with the new message Activity.
        await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)),
            cancellationToken);
    }

    protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded,
        ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
    {
        var welcomeCardContent = GetWelcomeCard(_urlsOptions.WillowLogo);

        foreach (var member in membersAdded)
            if (member.Id != turnContext.Activity.Recipient.Id)
            {
                var response = MessageFactory.Attachment(welcomeCardContent);
                await turnContext.SendActivityAsync(response, cancellationToken);
                await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>("DialogState"),
                    cancellationToken);
            }
    }


    protected override async Task OnTeamsSigninVerifyStateAsync(ITurnContext<IInvokeActivity> turnContext,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running dialog with Teams Signin Verify State Activity.");

        // Run the Dialog with the new Teams Signin Verify State  Activity.
        await _dialog.RunAsync(turnContext, _conversationState.CreateProperty<DialogState>(nameof(DialogState)),
            cancellationToken);
    }

    private static Attachment GetWelcomeCard(string logo)
    {
        var content = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
        {
            Body = new List<AdaptiveElement>
            {
                new AdaptiveImage(
                    logo),
                new AdaptiveTextBlock("Welcome to the Ops Bot"),
                new AdaptiveTextBlock("I can help with setting up access to willow resources")
            }
        };
        return new Attachment()
        {
            ContentType = AdaptiveCard.ContentType,
            Content = content
        };
    }
}
