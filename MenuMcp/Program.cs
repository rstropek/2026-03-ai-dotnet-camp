using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();
await builder.Build().RunAsync();

[McpServerToolType]
public static class MenuTools
{
    [McpServerTool, Description("Gets the complete menu card of the Döner shop including foods, drinks, extras, and sauces with prices in EUR.")]
    public static string GetMenu()
    {
        var menuPath = Path.Combine(AppContext.BaseDirectory, "menu.json");
        return File.ReadAllText(menuPath);
    }
}
