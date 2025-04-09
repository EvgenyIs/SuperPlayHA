using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Threading.Tasks;
using SuperPlayHA.Services;
using SuperPlayHA.Utils.Middlwares;
namespace SuperPlayHA
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.Configure(app =>
                    {
                        // Register exception handling middleware
                        app.UseMiddleware<ExceptionHandlingMiddleware>();

                        // Enable WebSocket support
                        app.UseWebSockets();

                        // Use custom route for testing WebSocket communication
                        app.Use(async (context, next) =>
                        {
                            if (context.Request.Path.StartsWithSegments("/Test") && context.WebSockets.IsWebSocketRequest)
                            {
                                var webSocketService = context.RequestServices.GetRequiredService<WebSocketService>();
                                using var ws = await context.WebSockets.AcceptWebSocketAsync();
                                await webSocketService.HandleConnection(ws);  // Pass the WebSocket connection to the service
                                return;
                            }
                            await next();
                        });

                        // You can add more middlewares like routing, authentication, etc.
                    });

                    // Set URL for the WebSocket server
                    webBuilder.UseUrls("http://localhost:5000");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Register services
                    //services.AddTransient<ExceptionHandlingMiddleware>(); // Make sure middleware is registered with correct lifetime
                    services.AddSingleton<WebSocketService>();  // Singleton for WebSocket service
                    services.AddSingleton<IPlayerService, PlayerService>();  // Singleton for player service
                    services.AddSingleton<IResourceService, ResourceService>();  // Singleton for resource service


                    services.AddHostedService<WebSocketService>();
                });
    }
}