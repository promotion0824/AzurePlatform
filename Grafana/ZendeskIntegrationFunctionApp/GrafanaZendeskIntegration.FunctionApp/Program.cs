using System.Net.Http.Headers;
using System.Text;
using Azure.Data.Tables;
using Azure.Identity;
using GrafanaZendeskIntegration.FunctionApp.Models;
using GrafanaZendeskIntegration.FunctionApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

var host = new HostBuilder()
.ConfigureFunctionsWorkerDefaults()
.ConfigureAppConfiguration((context, configBuilder) =>
{
    // Load configuration from environment variables
    var env = context.HostingEnvironment.EnvironmentName;
    configBuilder.SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    // Add Azure Key Vault
    var builtConfig = configBuilder.Build();
    var keyVaultName = builtConfig["KeyVaultName"];
    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
    configBuilder.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
})
.ConfigureServices((context, services) =>
{
    // Get the StorageClient and ZendeskClient configuration
    services.AddOptions<StorageClientOptions>()
        .Bind(context.Configuration.GetSection("StorageClientOptions"));
    services.AddOptions<ZendeskClientOptions>()
        .Bind(context.Configuration.GetSection("ZendeskClientOptions"));

    // Load the Field Mappings from the JSON file and add them to the DI container
    var webrootPath = Path.Combine(Environment.GetEnvironmentVariable("WEBROOT_PATH") ?? "C:/home/site/wwwroot",
        "Models/GrafanaZendeskFieldMapping.json");
    var json = System.IO.File.ReadAllText(webrootPath);
    var fieldMappings = JsonConvert.DeserializeObject<List<GrafanaZendeskFieldMapping>>(json);
    services.AddSingleton(fieldMappings);

    // Add the TableClient to the DI container
    var storageClientOptions = services.BuildServiceProvider().GetService<IOptions<StorageClientOptions>>().Value;
    var zendeskClientOptions = services.BuildServiceProvider().GetService<IOptions<ZendeskClientOptions>>().Value;
    var tableEndpoint = $"https://{storageClientOptions.StorageAccountName}.table.core.windows.net/";
    services.AddSingleton(new TableClient(new Uri(tableEndpoint), storageClientOptions.ZendeskMessageLogTableName, new DefaultAzureCredential()));

    // Add the HttpClient to the DI container
    var zendeskCredential = $"{zendeskClientOptions.ZendeskUserEmailAddress}/token:{zendeskClientOptions.ZendeskUserApiToken}";
    var zendeskCredentialBytes = Encoding.UTF8.GetBytes(zendeskCredential);
    var encodedZendeskCredential = Convert.ToBase64String(zendeskCredentialBytes);

    services.AddHttpClient("ZendeskClient", client =>
    {
        client.BaseAddress = new Uri(zendeskClientOptions.ZendeskTicketsApiUrl);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedZendeskCredential);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    });

    services.AddTransient<IStorageClient, StorageClient>();
    services.AddTransient<IZendeskClient, ZendeskClient>();
})
.Build();

host.Run();
