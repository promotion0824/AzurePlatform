// -----------------------------------------------------------------------
// <copyright file="ErrorResponseMessage.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Models;

public class ErrorResponseMessage
{
    public string Status { get; set; }

    public string Message { get; set; }

    public string DetailedMessage { get; set; }
}
