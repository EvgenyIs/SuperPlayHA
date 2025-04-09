using SuperPlayHA.Models;
using SuperPlayHA.Utils.Consts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StatusCodes = SuperPlayHA.Utils.Consts.StatusCodes;

namespace SuperPlayHA.Services
{
    public class WebSocketService : BackgroundService
    {
        private readonly ILogger<WebSocketService> _logger;
        private readonly IPlayerService _playerService;
        private readonly IResourceService _resourceService;

        public WebSocketService(ILogger<WebSocketService> logger, IPlayerService playerService, IResourceService resourceService)
        {
            _logger = logger;
            _playerService = playerService;
            _resourceService = resourceService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WebSocket Server is starting...");
            var httpListener = new System.Net.HttpListener();
            httpListener.Prefixes.Add("http://localhost:5077/");
            httpListener.Start();

            while (!stoppingToken.IsCancellationRequested)
            {
                var context = await httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    var wsContext = await context.AcceptWebSocketAsync(null);
                    _ = HandleConnection(wsContext.WebSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        public async Task HandleConnection(WebSocket socket)
        {
            _logger.LogInformation("New WebSocket connection established");
            var buffer = new byte[4096];

            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.Count == 0) continue;

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                _logger.LogInformation("Received: {Message}", message);

                var response = ProcessMessage(message, socket);
                if (!string.IsNullOrEmpty(response))
                {
                    await socket.SendAsync(Encoding.UTF8.GetBytes(response), WebSocketMessageType.Text, true, CancellationToken.None);
                    _logger.LogInformation("Sent: {Response}", response);
                }
            }
        }

        private string ProcessMessage(string message, WebSocket socket)
        {
            try
            {
                var request = JsonSerializer.Deserialize<GameMessageDto>(message);
                return request?.Type switch
                {
                    "Test" => "in Test WS",
                    "Login" => _playerService.HandleLogin(request, socket),
                    "UpdateResources" => _resourceService.HandleUpdateResources(request),
                    "SendGift" => _resourceService.HandleSendGift(request),
                    _ => JsonSerializer.Serialize(new ResponseDto { Status = StatusCodes.UnknownCommand })
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                return JsonSerializer.Serialize(new ResponseDto { Status = StatusCodes.Error, Message = ex.Message });
            }
        }
    }
}
