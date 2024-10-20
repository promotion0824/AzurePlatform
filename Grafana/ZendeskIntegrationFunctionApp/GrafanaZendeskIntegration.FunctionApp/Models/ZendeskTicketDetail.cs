// -----------------------------------------------------------------------
// <copyright file="ZendeskTicketDetail.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Models;

using System.Net;

// <summary>
//   Defines the ZendeskTicketDetail type.
// </summary>
public class ZendeskTicketDetail
{
    public HttpStatusCode StatusCode { get; set; } // Row Key

    public string ResponseBody { get; set; }

    public string TicketId { get; set; }
}
