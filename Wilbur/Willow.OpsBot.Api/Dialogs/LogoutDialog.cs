// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace Willow.OpsBot.Api.Dialogs;

public class LogoutDialog : ComponentDialog
{
    public LogoutDialog(string id, string connectionName)
        : base(id)
    {
        ConnectionName = connectionName;
    }

    protected string ConnectionName { get; private set; }

    protected override async Task<DialogTurnResult> OnBeginDialogAsync(DialogContext innerDc, object options,
        CancellationToken cancellationToken = default)
    {
        var result = await InterruptAsync(innerDc, cancellationToken);
        if (result != null) return result;

        return await base.OnBeginDialogAsync(innerDc, options, cancellationToken);
    }

    protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc,
        CancellationToken cancellationToken = default)
    {
        var result = await InterruptAsync(innerDc, cancellationToken);
        if (result != null) return result;

        return await base.OnContinueDialogAsync(innerDc, cancellationToken);
    }

    private async Task<DialogTurnResult?> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken)
    {
        if (innerDc.Context.Activity.Type == ActivityTypes.Message && innerDc.Context.Activity?.Text != null)
        {
            var text = innerDc.Context.Activity.Text.Trim().ToLowerInvariant();

            if (text.Trim() == "logout")
            {
                if (innerDc.Context.Adapter is IUserTokenProvider adapter)
                {
                    await adapter.SignOutUserAsync(innerDc.Context, ConnectionName, cancellationToken: cancellationToken);

                    await innerDc.Context.SendActivityAsync(MessageFactory.Text("You have been signed out."),
                        cancellationToken);
                }

                return await innerDc.CancelAllDialogsAsync(cancellationToken);
            }
        }

        return null;
    }
}
