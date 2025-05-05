// Chaos OPC UA Server in .NET 8 mit Microsoft.Extensions.Logging & Serilog

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using opcua.chaos.server;
using Serilog;


var builder = Host.CreateDefaultBuilder(args)
.UseSerilog((context, services, configuration) =>
{
    configuration
        .WriteTo.Console()
        .MinimumLevel.Information();
})
.ConfigureServices((hostContext, services) =>
{
    services.Configure<ChaosOptions>(hostContext.Configuration.GetSection("Chaos"));
    services.AddSingleton<ChaosServer>();
    services.AddSingleton<ChaosServerApplication>();
    services.AddHostedService<ChaosServerHostedService>();
});

await builder.RunConsoleAsync();
