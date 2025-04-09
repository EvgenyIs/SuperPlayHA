using SuperPlayHA.Models;
using SuperPlayHA.Utils.Consts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StatusCodes = SuperPlayHA.Utils.Consts.StatusCodes;

namespace SuperPlayHA.Services
{
    public interface IPlayerService
    {
        string HandleLogin(GameMessageDto request, WebSocket socket);
    }
    public class PlayerService : IPlayerService
    {
        private readonly ConcurrentDictionary<string, WebSocket> _connectedPlayers = new();
        private readonly ILogger<PlayerService> _logger;

        public PlayerService(ILogger<PlayerService> logger)
        {
            _logger = logger;
        }

        public string HandleLogin(GameMessageDto request, WebSocket socket)
        {
            if (_connectedPlayers.ContainsKey(request.DeviceId))
            {
                _logger.LogWarning("Player {DeviceId} already connected", request.DeviceId);
                return JsonSerializer.Serialize(new ResponseDto { Status = StatusCodes.AlreadyConnected });
            }

            var playerId = Guid.NewGuid().ToString();
            _connectedPlayers[request.DeviceId] = socket;
            _logger.LogInformation("Player {PlayerId} logged in", playerId);

            return JsonSerializer.Serialize(new ResponseDto { Status = StatusCodes.Success, PlayerId = playerId });
        }
    }
}
