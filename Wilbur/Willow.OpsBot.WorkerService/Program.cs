using Willow.OpsBot.Common.Services;
using Willow.OpsBot.WorkerService;
using Willow.OpsBot.WorkerService.Models.Options;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        var config = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        services.Configure<CleanupOptions>(config.GetSection(CleanupOptions.Position));
        services.AddSingleton<IAzureManagement, AzureManagement>();
        services.AddHostedService<Job>();
    });

var app = builder.Build();
app.Run();
