using System.Text.RegularExpressions;
using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Willow.OpsBot.Api.Exceptions;
using Willow.OpsBot.Api.Models;
using Willow.OpsBot.Api.Models.Options;
using Willow.OpsBot.Api.Services;
using Willow.OpsBot.Common.Models.Enums;
using Willow.OpsBot.Common.Services;

namespace Willow.OpsBot.Api.Dialogs;

public class FirewallAccessDialog : LogoutDialog
{
    internal const string ServerTypeMsgText = "Is this a SQL or PostgreSQL server?";
    private const string ServerNameMsgText = "What server would you like access to?";

    private const string ServerNameOtherMsgText = "Or type the name of another server:";

    internal const string ServerNotFoundMessage =
        "I can't access that server. Maybe check that you typed it correctly?";

    internal const string InvalidIpAddress = "Wasn't able to get a valid ip address";

    private const string UserIpMsgText = "What is your Ip Address?";

    private readonly ILogger _logger;
    private readonly AccessServersOptions _servers;
    private readonly IAzureManagement _azureManagement;
    private readonly IGraphServiceClientFactory _graphServiceClientFactory;
    private readonly UrlsOptions _urlsOptions;

    internal string LoginPromptDialogId = nameof(OAuthPrompt);

    public FirewallAccessDialog(IAzureManagement azureManagement, ILogger<FirewallAccessDialog> logger,
        IOptions<AccessServersOptions> servers, IConfiguration configuration,
        IGraphServiceClientFactory graphServiceClientFactory, IOptions<UrlsOptions> urlsOptions) : base(
        nameof(FirewallAccessDialog), configuration["ConnectionName"])
    {
        _azureManagement = azureManagement;
        _logger = logger;
        _graphServiceClientFactory = graphServiceClientFactory;
        _urlsOptions = urlsOptions.Value;
        _servers = servers.Value;

        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(new TextPrompt($"{nameof(TextPrompt)}_{nameof(IpValidation)}", IpValidation));
        AddDialog(new TextPrompt($"{nameof(TextPrompt)}_{nameof(CanAdministerServer)}", CanAdministerServer));

        AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));

        AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
        AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
        {
            ServerType,
            ServerName,
            UserIp,
            StartLoginStepAsync,
            LoginStepAsync
        }));
        AddDialog(new OAuthPrompt(LoginPromptDialogId,
            new OAuthPromptSettings
            {
                ConnectionName = ConnectionName,
                Text = "Please Sign In",
                Title = "Sign In",
                Timeout = 300000 // User has 5 minutes to login (1000 * 60 * 5)
            }));

        InitialDialogId = nameof(WaterfallDialog);
    }

    private FirewallAccessDetails GetPrefilledDetails(WaterfallStepContext stepContext)
    {
        var serverAccessDetails = (FirewallAccessDetails?)stepContext.Options ?? new FirewallAccessDetails();
        if (stepContext.Context?.Activity?.Text != null &&
            stepContext.Context.Activity.Text.Trim().StartsWith("fw/"))
        {
            var parts = stepContext.Context.Activity.Text.Split("/");
            try
            {
                var ip = parts[3].Trim();
                if (IsValidIp(ip)) serverAccessDetails.Ip = ip;
                serverAccessDetails.Prefilled = true;
                serverAccessDetails.Server = parts[2].Trim();
                serverAccessDetails.Type = Enum.Parse<ServerTypes>(parts[1].Trim());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not decode prefilled info");
            }
        }

        return serverAccessDetails;
    }

    private async Task<DialogTurnResult> ServerType(WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        var serverAccessDetails = GetPrefilledDetails(stepContext);

        if (serverAccessDetails.Type == 0)
        {
            var choicesList = Enum.GetNames<ServerTypes>();
            _logger.LogDebug("Asking for server type");
            var promptMessage =
                MessageFactory.Text(ServerTypeMsgText, ServerTypeMsgText, InputHints.ExpectingInput);
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = promptMessage,
                Choices = ChoiceFactory.ToChoices(choicesList),
                Style = ListStyle.HeroCard
            }, cancellationToken);
        }

        return await stepContext.NextAsync(new FoundChoice { Value = serverAccessDetails.Type.ToString() },
            cancellationToken);
    }

    private async Task<DialogTurnResult> ServerName(WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        var serverAccessDetails = (FirewallAccessDetails)stepContext.Options;
        serverAccessDetails.Type = Enum.Parse<ServerTypes>(((FoundChoice)stepContext.Result).Value);

        if (serverAccessDetails.Server == null)
        {
            _logger.LogDebug("Asking for server name for {Type}", serverAccessDetails.Type);
            var choicesList = ServerNames(serverAccessDetails.Type);


            var content = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Color = AdaptiveTextColor.Good,
                        Text = serverAccessDetails.Type.ToString(),
                        Size = AdaptiveTextSize.Large
                    },
                    new AdaptiveChoiceSetInput
                    {
                        Label = ServerNameMsgText,
                        Id = nameof(ServerSelectionResult.ServerSelect),
                        Style = AdaptiveChoiceInputStyle.Compact,
                        Choices = choicesList.Select(choice => new AdaptiveChoice
                        {
                            Title = choice,
                            Value = choice
                        }).ToList()
                    },
                    new AdaptiveTextInput()
                    {
                        Label = ServerNameOtherMsgText,
                        Spacing = AdaptiveSpacing.None,
                        Id = nameof(ServerSelectionResult.OtherSelect)
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = "Submit",
                        Type = AdaptiveSubmitAction.TypeName
                    }
                }
            };
            var attachment = MessageFactory.Attachment(new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JObject.FromObject(content)
            }, inputHint: InputHints.ExpectingInput);
            return await stepContext.PromptAsync($"{nameof(TextPrompt)}_{nameof(CanAdministerServer)}",
                new PromptOptions
                {
                    Prompt = (Activity)attachment,
                    Validations = serverAccessDetails.Type
                }, cancellationToken);
        }

        return await stepContext.NextAsync(serverAccessDetails.Server, cancellationToken);
    }

    private async Task<DialogTurnResult> UserIp(WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        var serverAccessDetails = (FirewallAccessDetails)stepContext.Options;


        serverAccessDetails.Server = GetServerNameFromSubmission(stepContext.Context, stepContext.Result);

        if (serverAccessDetails.Ip == null)
        {
            _logger.LogDebug("Asking for user ip to add");

            var content = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock(UserIpMsgText)
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Url = new Uri(_urlsOptions.IpLookupSite),
                        Title = _urlsOptions.IpLookupSite
                    }
                }
            };
            var attachment = MessageFactory.Attachment(new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = JObject.FromObject(content)
            }, inputHint: InputHints.ExpectingInput);
            return await stepContext.PromptAsync($"{nameof(TextPrompt)}_{nameof(IpValidation)}", new PromptOptions
            {
                Prompt = (Activity)attachment
            }, cancellationToken);
        }

        return await stepContext.NextAsync(serverAccessDetails.Ip, cancellationToken);
    }

    internal Task<bool> IpValidation(PromptValidatorContext<string> promptContext,
        CancellationToken cancellationToken)
    {
        var isValidIp = IsValidIp(promptContext.Context?.Activity?.Text?.Trim());
        if (!isValidIp && promptContext.AttemptCount > 2) throw new TooManyAttemptsException(InvalidIpAddress);
        return Task.FromResult(isValidIp);
    }

    internal async Task<bool> CanAdministerServer(PromptValidatorContext<string> promptContext,
        CancellationToken cancellationToken)
    {
        var msg = GetServerNameFromSubmission(promptContext.Context);
        var validationType = (ServerTypes)promptContext.Options.Validations;
        if (string.IsNullOrWhiteSpace(msg) || validationType is 0) return false;

        var canAccess = await _azureManagement.AccessibleServer(validationType, msg);

        if (!canAccess && promptContext.Context != null)
            await promptContext.Context.SendActivityAsync(ServerNotFoundMessage, cancellationToken: cancellationToken);
        if (!canAccess && promptContext.AttemptCount > 2)
            throw new TooManyAttemptsException($"Wasn't able to get a valid server name for {validationType}");

        return canAccess;
    }

    private static string GetServerNameFromSubmission(ITurnContext turn, object? stepContextResult = null)
    {
        if (stepContextResult != null) return (string)stepContextResult;
        if (!string.IsNullOrWhiteSpace(turn?.Activity.Text)) return turn.Activity.Text;
        if (turn?.Activity.Value == null) return string.Empty;
        var result =
            JsonConvert.DeserializeObject<ServerSelectionResult>(turn.Activity.Value.ToString() ?? string.Empty);
        return (string.IsNullOrWhiteSpace(result?.OtherSelect)
            ? result?.ServerSelect
            : result.OtherSelect) ?? string.Empty;
    }


    internal static bool IsValidIp(string? ip)
    {
        if (ip == null) return false;
        // https://chrisjwarwick.wordpress.com/2012/09/16/more-regular-expressions-regex-for-ip-v4-addresses/
        var regex = new Regex(
            "^\\b(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)(\\.(25[0-5]|2[0-4]\\d|1\\d\\d|[1-9]?\\d)){3}\\b$");
        var match = regex.Match(ip);
        return match.Success;
    }

    private async Task<DialogTurnResult> StartLoginStepAsync(WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        var serverAccessDetails = (FirewallAccessDetails)stepContext.Options;
        serverAccessDetails.Ip = ((string?)stepContext.Result ?? string.Empty).Trim();

        return await stepContext.BeginDialogAsync(LoginPromptDialogId, null, cancellationToken);
    }

    private async Task<DialogTurnResult> LoginStepAsync(WaterfallStepContext stepContext,
        CancellationToken cancellationToken)
    {
        var serverAccessDetails = (FirewallAccessDetails)stepContext.Options;

        var tokenResponse = (TokenResponse?)stepContext.Result;
        if (tokenResponse != null)
        {
            var client = _graphServiceClientFactory.GetAuthenticatedClient(tokenResponse.Token);
            var me = await client.Me.Request().GetAsync(cancellationToken);

            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text(
                    $"Adding access to {serverAccessDetails.Type} - {serverAccessDetails.Server} for {serverAccessDetails.Ip}"),
                cancellationToken);
            await _azureManagement.AddUserIpToServer(serverAccessDetails.Type, serverAccessDetails.Server!,
                serverAccessDetails.Ip!.Trim(), me.UserPrincipalName);
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text(
                    $"Your access was added as {me.DisplayName}  ({me.UserPrincipalName}). To logout, type 'logout'."),
                cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Access will be removed after one day"),
                cancellationToken);

            if (!serverAccessDetails.Prefilled)
                await SendHintForNextTime(stepContext, serverAccessDetails, cancellationToken);

            return await stepContext.EndDialogAsync(serverAccessDetails, cancellationToken);
        }

        await stepContext.Context.SendActivityAsync(
            MessageFactory.Text("Login was not successful please try again."), cancellationToken);
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }

    private static async Task SendHintForNextTime(WaterfallStepContext stepContext,
        FirewallAccessDetails serverAccessDetails, CancellationToken cancellationToken)
    {
        var choices = new List<string>
            { $"fw/{serverAccessDetails.Type}/{serverAccessDetails.Server}/{serverAccessDetails.Ip}" };
        var content = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
        {
            Body = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock("Next time you need access you can skip the chatter"),
                new AdaptiveTextBlock("Just tell me what you need with direct commands")
            },
            Actions = choices.Select(choice => new AdaptiveSubmitAction
            {
                Title = choice,

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
        await stepContext.Context.SendActivityAsync((Activity)attachment, cancellationToken);
    }


    private string[] ServerNames(ServerTypes type)
    {
        return type switch
        {
            ServerTypes.SqlServer => _servers.SqlServer,
            ServerTypes.PostgreSql => _servers.PostgreSql,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Server type was invalid")
        };
    }
}
