﻿using Microsoft.Bot.Builder.Dialogs;
using Moq;

namespace Willow.OpsBot.Api.Tests.Common;

public static class SimpleMockFactory
{
    public static Mock<T> CreateMockDialog<T>(object? expectedResult = null, params object[] constructorParams)
        where T : Dialog
    {
        var mockDialog = new Mock<T>(constructorParams);
        var mockDialogNameTypeName = typeof(T).Name;
        mockDialog
            .Setup(x => x.BeginDialogAsync(It.IsAny<DialogContext>(), It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .Returns(async (DialogContext dialogContext, object options, CancellationToken cancellationToken) =>
            {
                await dialogContext.Context.SendActivityAsync(
                    $"{mockDialogNameTypeName} mock invoked with options: {options}",
                    cancellationToken: cancellationToken);
                return await dialogContext.EndDialogAsync(expectedResult, cancellationToken);
            });

        return mockDialog;
    }
}
