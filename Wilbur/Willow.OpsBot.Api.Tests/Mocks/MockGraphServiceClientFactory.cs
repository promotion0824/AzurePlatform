using System.Net;
using System.Net.Http.Headers;
using Microsoft.Graph;
using MockHttpClient;
using Willow.OpsBot.Api.Services;

namespace Willow.OpsBot.Api.Tests.Mocks;

public class MockGraphServiceClientFactory : IGraphServiceClientFactory
{
    public GraphServiceClient GetAuthenticatedClient(string authToken)
    {
        var mockGraph = "https://graph.microsoft.com/v1.0";
        var mockHttpClient = new MockHttpClient.MockHttpClient();
        mockHttpClient.When(mockGraph).Then(HttpStatusCode.OK);
        mockHttpClient.When(HttpMethod.Get, $"{mockGraph}/me").Then(req => new HttpResponseMessage()
            .WithJsonContent(new
            {
                // @odata.context =  "http = //graph.microsoft.com/v1.0/$metadata#users/$entity",
                // @odata.id =  "http = //graph.microsoft.com/v2/dcd219dd-bc68-4b9b-bf0b-4a33a796be35/directoryObjects/48d31887-5fad-4d73-a9f5-3c356e68a038/Microsoft.DirectoryServices.User",
                businessPhones = new List<string> { "+1 412 555 0109" },
                displayName = "Megan Bowen",
                givenName = "Megan",
                jobTitle = "Auditor",
                mail = "MeganB@M365x214355.onmicrosoft.com",
                // mobilePhone =  null,
                officeLocation = "12/1110",
                preferredLanguage = "en-US",
                surname = "Bowen",
                userPrincipalName = "MeganB@M365x214355.onmicrosoft.com",
                id = "48d31887-5fad-4d73-a9f5-3c356e68a038"
            }));

        var client = new GraphServiceClient(mockHttpClient, mockGraph)
        {
            AuthenticationProvider = new DelegateAuthenticationProvider(
                requestMessage =>
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", authToken);
                    requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");
                    return Task.CompletedTask;
                })
        };

        return client;
    }
}
