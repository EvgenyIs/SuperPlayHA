using SuperPlayHA.Models;
using SuperPlayHA.Utils.Consts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using StatusCodes = SuperPlayHA.Utils.Consts.StatusCodes;

namespace SuperPlayHA.Services
{
    public interface IResourceService
    {
        string HandleUpdateResources(GameMessageDto request);
        string HandleSendGift(GameMessageDto request);
    }
    public class ResourceService : IResourceService
    {
        private readonly ConcurrentDictionary<string, int> _playerResources = new();
        private readonly ILogger<ResourceService> _logger;

        public ResourceService(ILogger<ResourceService> logger)
        {
            _logger = logger;
        }

        public string HandleUpdateResources(GameMessageDto request)
        {
            if (!_playerResources.TryGetValue(request.PlayerId, out var currentBalance))
            {
                _logger.LogWarning("Player {PlayerId} not found", request.PlayerId);
                return JsonSerializer.Serialize(new ResponseDto { Status = StatusCodes.PlayerNotFound });
            }

            _playerResources[request.PlayerId] = currentBalance + request.ResourceValue;
            _logger.LogInformation("Updated {ResourceType} for Player {PlayerId}: {ResourceValue}", request.ResourceType, request.PlayerId, request.ResourceValue);

            return JsonSerializer.Serialize(new ResponseDto { Status = StatusCodes.Success, NewBalance = _playerResources[request.PlayerId] });
        }

        public string HandleSendGift(GameMessageDto request)
        {
            if (!_playerResources.TryGetValue(request.PlayerId, out var senderBalance) ||
                !_playerResources.TryGetValue(request.FriendPlayerId, out var receiverBalance))
            {
                _logger.LogWarning("One of the players not found: {PlayerId} -> {FriendPlayerId}", request.PlayerId, request.FriendPlayerId);
                return JsonSerializer.Serialize(new ResponseDto { Status = StatusCodes.PlayerNotFound });
            }

            if (senderBalance < request.ResourceValue)
            {
                _logger.LogWarning("Player {PlayerId} has insufficient funds", request.PlayerId);
                return JsonSerializer.Serialize(new ResponseDto { Status = StatusCodes.InsufficientFunds });
            }

            _playerResources[request.PlayerId] = senderBalance - request.ResourceValue;
            _playerResources[request.FriendPlayerId] = receiverBalance + request.ResourceValue;
            _logger.LogInformation("Player {PlayerId} sent {ResourceValue} {ResourceType} to {FriendPlayerId}", request.PlayerId, request.ResourceValue, request.ResourceType, request.FriendPlayerId);

            return JsonSerializer.Serialize(new ResponseDto { Status = StatusCodes.Success });
        }
    }
}
