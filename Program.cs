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
                        app.UseMiddleware<ExceptionHandlingMiddleware>();

                        app.UseWebSockets();

      
                        app.Use(async (context, next) =>
                        {
                            if (context.Request.Path.StartsWithSegments("/Test") && context.WebSockets.IsWebSocketRequest)
                            {
                                var webSocketService = context.RequestServices.GetRequiredService<WebSocketService>();
                                using var ws = await context.WebSockets.AcceptWebSocketAsync();
                                await webSocketService.HandleConnection(ws);  
                                return;
                            }
                            await next();
                        });
 
                    });

                    webBuilder.UseUrls("http://localhost:5000");
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<WebSocketService>();  
                    services.AddSingleton<IPlayerService, PlayerService>();  
                    services.AddSingleton<IResourceService, ResourceService>();  

                    services.AddHostedService<WebSocketService>();
                });
    }
}