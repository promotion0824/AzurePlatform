using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.Identity;
using Microsoft.Azure.Management.PostgreSQL;
using Microsoft.Azure.Management.ResourceGraph;
using Microsoft.Azure.Management.ResourceGraph.Models;
using Microsoft.Azure.Management.Sql;
using Microsoft.Azure.Management.Sql.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Microsoft.Rest.Azure;
using Willow.OpsBot.Common.Extensions;
using Willow.OpsBot.Common.Models;
using Willow.OpsBot.Common.Models.AzGraph;
using Willow.OpsBot.Common.Models.Enums;

[assembly: InternalsVisibleTo("Willow.OpsBot.Common.Tests")]

namespace Willow.OpsBot.Common.Services;

public class AzureManagement : IAzureManagement
{
    private readonly ILogger<AzureManagement> _logger;
    public const string OpsBotIdentifier = "_OB_";

    public AzureManagement(ILogger<AzureManagement> logger)
    {
        _logger = logger;
    }

    public async Task RemoveTimeBasedFirewallRule(List<string>? serverNameExclusions)
    {
        serverNameExclusions ??= new List<string>();
        var serviceClientCredentials = await GetServiceClientCredentials();
        var queryResult = await QueryAzureGraph(
            $"Resources | where type == '{ServerTypes.PostgreSql.GetAzureType()}' or type == '{ServerTypes.SqlServer.GetAzureType()}'  | project type, subscriptionId, resourceGroup, name",
            serviceClientCredentials);
        var resourceLocations = JsonSerializer.Deserialize<List<ResourceLocation>>(
            queryResult.Body.Data.ToString() ?? "[]",
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        foreach (var resource in (resourceLocations ?? new List<ResourceLocation>()).Where(r =>
                     r.Name != null && !serverNameExclusions.Contains(r.Name)))
            switch ((resource.Type ?? throw new Exception("No Resource Type")).GetServerType())
            {
                case ServerTypes.SqlServer:
                    await RemoveOldSqlServerFirewallRule(serviceClientCredentials, resource);
                    break;
                case ServerTypes.PostgreSql:
                    await RemoveOldPostgreSqlFirewallRule(serviceClientCredentials, resource);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unsuported type {resource.Type}");
            }
    }
    private static string RemoveFullyQualifiedNameFromServerName(string name) =>
        name.Replace(".postgres.database.azure.com", "").Replace(".database.windows.net", "");

    public async Task<bool> AccessibleServer(ServerTypes type, string name)
    {
        var serviceClientCredentials = await GetServiceClientCredentials();
        var resourceLocations = await GetResources(type, RemoveFullyQualifiedNameFromServerName(name), serviceClientCredentials);
        return resourceLocations is { Count: 1 };
    }

    public async Task AddUserIpToServer(ServerTypes type, string name, string ip, string userName)
    {
        var serverName = RemoveFullyQualifiedNameFromServerName(name);
        var serviceClientCredentials = await GetServiceClientCredentials();
        var resourceLocations = await GetResources(type, serverName, serviceClientCredentials);
        var server = resourceLocations?.FirstOrDefault() ?? throw new Exception($"Unable to find server {name}");

        var ruleName = MakeRuleName(userName);

        switch (type)
        {
            case ServerTypes.SqlServer:
                await AddSqlServerFirewallRule(serverName, ip, serviceClientCredentials, server, ruleName);
                break;
            case ServerTypes.PostgreSql:
                await AddPostgreSqlFirewallRule(serverName, ip, serviceClientCredentials, server, ruleName);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private static async Task<List<ResourceLocation>?> GetResources(ServerTypes type, string name,
        TokenCredentials clientCredentials)
    {
        var queryResult = await QueryAzureGraph(
            $"Resources | where type == '{type.GetAzureType()}' | where name == '{name}' | project type, subscriptionId, resourceGroup",
            clientCredentials);

        var resourceLocations = JsonSerializer.Deserialize<List<ResourceLocation>>(
            queryResult.Body.Data.ToString() ?? "[]",
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return resourceLocations;
    }

    internal static string MakeRuleName(string userName)
    {
        var ruleName =
            $"{userName.Split("@").FirstOrDefault()}{OpsBotIdentifier}{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        return ruleName;
    }

    private static async Task<TokenCredentials> GetServiceClientCredentials()
    {
        var credentials = new DefaultAzureCredential();
        var token = await credentials.GetTokenAsync(
            new Azure.Core.TokenRequestContext(new[] { "https://management.azure.com/.default" }));
        var serviceClientCredentials = new TokenCredentials(token.Token);
        return serviceClientCredentials;
    }

    private static async Task<AzureOperationResponse<QueryResponse>> QueryAzureGraph(string query,
        ServiceClientCredentials serviceClientCredentials)
    {
        var graphClient = new ResourceGraphClient(serviceClientCredentials);


        var azResources = await graphClient.ResourcesWithHttpMessagesAsync(
            new QueryRequest(
                query));
        return azResources;
    }

    private static async Task AddSqlServerFirewallRule(string name, string ip,
        ServiceClientCredentials serviceClientCredentials,
        ResourceLocation server, string? ruleName)
    {
        var client = new SqlManagementClient(serviceClientCredentials) { SubscriptionId = server.SubscriptionId };
        await client.FirewallRules.CreateOrUpdateWithHttpMessagesAsync(server.ResourceGroup, name, ruleName,
            new FirewallRule(name: ruleName, startIpAddress: ip, endIpAddress: ip));
    }

    private static async Task AddPostgreSqlFirewallRule(string name, string ip,
        ServiceClientCredentials serviceClientCredentials,
        ResourceLocation server, string? ruleName)
    {
        var client = new PostgreSQLManagementClient(serviceClientCredentials)
            { SubscriptionId = server.SubscriptionId };
        await client.FirewallRules.CreateOrUpdateWithHttpMessagesAsync(server.ResourceGroup, name, ruleName,
            new Microsoft.Azure.Management.PostgreSQL.Models.FirewallRule(name: ruleName, startIpAddress: ip,
                endIpAddress: ip));
    }

    private async Task RemoveOldSqlServerFirewallRule(ServiceClientCredentials serviceClientCredentials,
        ResourceLocation server)
    {
        var client = new SqlManagementClient(serviceClientCredentials) { SubscriptionId = server.SubscriptionId };
        var rules = await client.FirewallRules.ListByServerWithHttpMessagesAsync(server.ResourceGroup, server.Name);

        await CleanupRules(server,
            rules.Body.Select(r => new GenericFirewallRule(r.Id, r.Name, r.Type, r.StartIpAddress, r.EndIpAddress)),
            ruleName => client.FirewallRules.DeleteWithHttpMessagesAsync(server.ResourceGroup, server.Name,
                ruleName));
    }

    private async Task RemoveOldPostgreSqlFirewallRule(ServiceClientCredentials serviceClientCredentials,
        ResourceLocation server)
    {
        var client = new PostgreSQLManagementClient(serviceClientCredentials)
            { SubscriptionId = server.SubscriptionId };
        var rules = await client.FirewallRules.ListByServerWithHttpMessagesAsync(server.ResourceGroup, server.Name);

        await CleanupRules(server,
            rules.Body.Select(r => new GenericFirewallRule(r.Id, r.Name, r.Type, r.StartIpAddress, r.EndIpAddress)),
            ruleName => client.FirewallRules.DeleteWithHttpMessagesAsync(server.ResourceGroup, server.Name,
                ruleName));
    }

    internal async Task CleanupRules(ResourceLocation server, IEnumerable<GenericFirewallRule> rules,
        Func<string, Task> removeRule)
    {
        _logger.LogInformation("Running cleanup for {ServerName} of {Type} in {Subscription}/{ResourceGroup}",
            server.Name, server.Type, server.SubscriptionId, server.ResourceGroup);
        foreach (var rule in rules)
            if (RuleShouldBeRemoved(rule.Name))
            {
                _logger.LogInformation(
                    "Removing {RuleName} from {ServerName} of {Type} in {Subscription}/{ResourceGroup}", rule.Name,
                    server.Name, server.Type, server.SubscriptionId, server.ResourceGroup);
                try
                {
                    await removeRule(rule.Name);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,
                        "Failed to remove {RuleName} from {ServerName}/{Type} in {Subscription}/{ResourceGroup}",
                        rule.Name,
                        server.Name, server.Type, server.SubscriptionId, server.ResourceGroup);
                }
            }
    }

    internal static bool RuleShouldBeRemoved(string ruleName)
    {
        try
        {
            if (ruleName.Contains("_WAB_"))
            {
                var dateRegex = new Regex(@"(\d{4}\d{1,2}\d{1,2}\d{1,6})");
                var matches = dateRegex.Match(ruleName);

                if (!matches.Success) return false;
                var date = DateTime.ParseExact(matches.Groups[0].Value, "yyyyMMddHHmmss", new DateTimeFormatInfo());
                return date < DateTime.UtcNow.AddDays(-1); // Cleanup after 24 hours
            }
            else
            {
                if (ruleName.Contains(OpsBotIdentifier))
                {
                    var dateRegex = new Regex(@"(\d{1,10})");
                    var matches = dateRegex.Match(ruleName);

                    if (!matches.Success) return false;
                    var date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(matches.Groups[0].Value));
                    return date < DateTime.UtcNow.AddDays(-1); // Cleanup after 24 hours
                }
                else
                {
                    var dateRegex = new Regex(@"(\d{4}-\d{1,2}-\d{1,2})");
                    var matches = dateRegex.Match(ruleName);

                    if (!matches.Success) return false;
                    var dateString = matches.Groups[0];
                    var date = DateTime.ParseExact(dateString.Value, "yyyy-M-d", new DateTimeFormatInfo());
                    return date < DateTime.UtcNow.AddMonths(-1); // Cleanup after 1 Month
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
