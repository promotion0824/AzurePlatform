// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with CoreBot .NET Template version v4.14.1

using System.Runtime.CompilerServices;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Schema;
using Willow.OpsBot.Api.Exceptions;
[assembly:InternalsVisibleTo("Willow.OpsBot.Api.Tests")]

namespace Willow.OpsBot.Api;

public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
{
    public AdapterWithErrorHandler(IConfiguration configuration, ILogger<AdapterWithErrorHandler> logger,
        TelemetryInitializerMiddleware telemetryInitializerMiddleware, IBotTelemetryClient botTelemetryClient,
        ConversationState? conversationState = null)
        : base(configuration, logger)
    {
        Use(telemetryInitializerMiddleware);

        //Use telemetry client so that we can trace exceptions into Application Insights

        OnTurnError = async (turnContext, exception) =>
        {
            if (exception is TooManyAttemptsException)
            {
                var errorMessageText = "Too many attempts lets start over fresh";
                var errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.ExpectingInput);
                await turnContext.SendActivityAsync(errorMessage);
            }
            else
            {
                var properties = new Dictionary<string, string>
                    {{"Bot exception caught in", $"{nameof(AdapterWithErrorHandler)} - {nameof(OnTurnError)}"}};
                botTelemetryClient.TrackException(exception, properties);

                // Log any leaked exception from the application.
                logger.LogError(exception, "[OnTurnError] unhandled error : {Message}", exception.Message);

                // Send a message to the user
                var errorMessageText = "The bot encountered an error or bug.";
                var errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.ExpectingInput);
                await turnContext.SendActivityAsync(errorMessage);

                errorMessageText = "Please let the Platform team know if the issue persists.";
                errorMessage = MessageFactory.Text(errorMessageText, errorMessageText, InputHints.ExpectingInput);
                await turnContext.SendActivityAsync(errorMessage);
            }

            if (conversationState != null)
                try
                {
                    // Delete the conversationState for the current conversation to prevent the
                    // bot from getting stuck in a error-loop caused by being in a bad state.
                    // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                    await conversationState.DeleteAsync(turnContext);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Exception caught on attempting to Delete ConversationState : {Message}", e.Message);
                }

            // Send a trace activity, which will be displayed in the Bot Framework Emulator
            await turnContext.TraceActivityAsync("OnTurnError Trace", exception.Message,
                "https://www.botframework.com/schemas/error", "TurnError");
        };
    }
}
