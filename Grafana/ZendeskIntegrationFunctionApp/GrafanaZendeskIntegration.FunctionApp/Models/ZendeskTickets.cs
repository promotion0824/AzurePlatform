
public class ZendeskTickets
{
    public Result[] results { get; set; }
    public object facets { get; set; }
    public object next_page { get; set; }
    public object previous_page { get; set; }
    public int count { get; set; }
}

public class Result
{
    public string url { get; set; }
    public int id { get; set; }
    public string external_id { get; set; }
    public Via via { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public int generated_timestamp { get; set; }
    public string type { get; set; }
    public string subject { get; set; }
    public string raw_subject { get; set; }
    public string description { get; set; }
    public string priority { get; set; }
    public string status { get; set; }
    public string recipient { get; set; }
    public long requester_id { get; set; }
    public long submitter_id { get; set; }
    public long? assignee_id { get; set; }
    public long? organization_id { get; set; }
    public long group_id { get; set; }
    public long?[] collaborator_ids { get; set; }
    public long?[] follower_ids { get; set; }
    public long?[] email_cc_ids { get; set; }
    public object forum_topic_id { get; set; }
    public object problem_id { get; set; }
    public bool has_incidents { get; set; }
    public bool is_public { get; set; }
    public object due_at { get; set; }
    public string[] tags { get; set; }
    public Custom_Fields[] custom_fields { get; set; }
    public Satisfaction_Rating satisfaction_rating { get; set; }
    public object[] sharing_agreement_ids { get; set; }
    public int custom_status_id { get; set; }
    public Field[] fields { get; set; }
    public object[] followup_ids { get; set; }
    public long ticket_form_id { get; set; }
    public long brand_id { get; set; }
    public bool allow_channelback { get; set; }
    public bool allow_attachments { get; set; }
    public bool from_messaging_channel { get; set; }
    public string result_type { get; set; }
}

public class Via
{
    public string channel { get; set; }
    public Source source { get; set; }
}

public class Source
{
    public From from { get; set; }
    public To to { get; set; }
    public object rel { get; set; }
}

public class From
{
    public string address { get; set; }
    public string name { get; set; }
}

public class To
{
    public string name { get; set; }
    public string address { get; set; }
}

public class Satisfaction_Rating
{
    public string score { get; set; }
}

public class Custom_Fields
{
    public long id { get; set; }
    public string value { get; set; }
}

public class Field
{
    public long id { get; set; }
    public string value { get; set; }
}
