using System.Net.Http.Headers;
using Microsoft.Graph;

namespace Willow.OpsBot.Api.Services;

public class GraphServiceClientFactory : IGraphServiceClientFactory
{
    public GraphServiceClient GetAuthenticatedClient(string authToken)
    {
        var graphClient = new GraphServiceClient(
            new DelegateAuthenticationProvider(
                requestMessage =>
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", authToken);
                    requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");
                    return Task.CompletedTask;
                }));
        return graphClient;
    }
}

public interface IGraphServiceClientFactory
{
    GraphServiceClient GetAuthenticatedClient(string authToken);
}
