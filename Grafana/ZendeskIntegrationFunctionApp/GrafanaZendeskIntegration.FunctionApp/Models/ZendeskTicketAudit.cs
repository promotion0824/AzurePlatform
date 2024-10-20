namespace GrafanaZendeskIntegration.FunctionApp.Models;

public class ZendeskTicketAudits
{
    public Audit[] audits { get; set; }

    public object next_page { get; set; }

    public object previous_page { get; set; }

    public int count { get; set; }
}

public class Audit
{
    public long id { get; set; }

    public int ticket_id { get; set; }

    public Via via { get; set; }
}

public class Via
{
    public Source source { get; set; }
}

public class Source
{
    public From from { get; set; }

    public string rel { get; set; }
}

public class From
{
    public string ticket_id { get; set; }
}
