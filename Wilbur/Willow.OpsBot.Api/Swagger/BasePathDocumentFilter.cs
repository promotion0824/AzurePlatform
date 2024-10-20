using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Willow.OpsBot.Api.Swagger;

internal class BasePathDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var url = Environment.GetEnvironmentVariable("APP_URL");
        if (!string.IsNullOrWhiteSpace(url)) swaggerDoc.Servers = new List<OpenApiServer> { new() { Url = url } };
    }
}
