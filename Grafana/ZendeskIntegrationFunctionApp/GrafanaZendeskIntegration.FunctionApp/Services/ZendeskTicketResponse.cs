// -----------------------------------------------------------------------
// <copyright file="ZendeskClient.ShowZendeskTicket.cs" company="Willow, Inc">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace GrafanaZendeskIntegration.FunctionApp.Services
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class ZendeskTicketResponse
    {
        public static ZendeskTicketResponse CreateInstance(string jsonString)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
            };
            settings.Converters.Add(new ZendeskTicketResponseConverter());
            var zendeskTicketDetail = JsonConvert.DeserializeObject<ZendeskTicketResponse>(jsonString, settings);
            return zendeskTicketDetail;
        }
    }

    [JsonConverter(typeof(ZendeskTicketResponseConverter))]
    public class ZendeskTicket : ZendeskTicketResponse
    {
        public Ticket ticket { get; set; }
    }

    public class Ticket
    {
        public long assignee_id { get; set; }

        public List<long> collaborator_ids { get; set; }

        public DateTime created_at { get; set; }

        public List<CustomField> custom_fields { get; set; }

        public long custom_status_id { get; set; }

        public string description { get; set; }

        public object due_at { get; set; }

        public string external_id { get; set; }

        public List<long> follower_ids { get; set; }

        public bool from_messaging_channel { get; set; }

        public long group_id { get; set; }

        public bool has_incidents { get; set; }

        public long id { get; set; }

        public long organization_id { get; set; }

        public string priority { get; set; }

        public string raw_subject { get; set; }

        public string recipient { get; set; }

        public long requester_id { get; set; }

        public List<long> sharing_agreement_ids { get; set; }

        public string status { get; set; }

        public string subject { get; set; }

        public long submitter_id { get; set; }

        public List<string> tags { get; set; }

        public string type { get; set; }

        public DateTime updated_at { get; set; }

        public string url { get; set; }
    }

    public class CustomField
    {
        public long id { get; set; }

        public string value { get; set; }
    }

    public class SimpleError : ZendeskTicketResponse
    {
        public string error { get; set; }

        public string description { get; set; }
    }

    public class DetailedError : ZendeskTicketResponse
    {
        public Error error { get; set; }
    }

    public class Error
    {
        public string title { get; set; }

        public string message { get; set; }
    }
}
