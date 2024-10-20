using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.OpenApi.Models;
using Willow.OpsBot.Api;
using Willow.OpsBot.Api.Bots;
using Willow.OpsBot.Api.Dialogs;
using Willow.OpsBot.Api.Logging;
using Willow.OpsBot.Api.Models.Options;
using Willow.OpsBot.Api.Services;
using Willow.OpsBot.Api.Swagger;
using Willow.OpsBot.Common.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureAppConfiguration((_, config) =>
{
    var builtConfig = config.Build();
    var secretClient = new SecretClient(new Uri(builtConfig["KeyVault:VaultUri"]), new DefaultAzureCredential());
    config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
});

builder.Host.ConfigureLogging(l =>
{
    l.ClearProviders();
    l.AddConsole();
    l.AddApplicationInsights();
});


builder.Services.AddHealthChecks();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.DocumentFilter<BasePathDocumentFilter>();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Willow.OpsBot.Api", Version = "v1" });
});


builder.Services.AddCors();

builder.Services.Configure<AccessServersOptions>(builder.Configuration.GetSection(AccessServersOptions.Position));
builder.Services.Configure<UrlsOptions>(builder.Configuration.GetSection(UrlsOptions.Position));
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
builder.Services.AddSingleton<IStorage, MemoryStorage>();
builder.Services.AddSingleton<UserState>();
builder.Services.AddSingleton<ConversationState>();

builder.Services.AddSingleton<AccessDialog>();
builder.Services.AddSingleton<FirewallAccessDialog>();
builder.Services.AddSingleton<MainDialog>();
builder.Services.AddSingleton<IAzureManagement, AzureManagement>();

builder.Services.AddSingleton<IGraphServiceClientFactory, GraphServiceClientFactory>();

builder.Services.AddTransient<IBot, TeamsOpsBot<MainDialog>>();

builder.Services.ConfigureApplicationInsightsTelemetry();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Willow.OpsBot.Api v1"));
}

app.UseRouting();

app.UseAuthorization();


app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health");
    endpoints.MapControllers();
});

app.Run();
