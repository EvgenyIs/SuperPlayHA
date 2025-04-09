using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperPlayHA.Models
{
    public class GameMessageDto
    {
        public string Type { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string PlayerId { get; set; } = string.Empty;
        public string FriendPlayerId { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
        public int ResourceValue { get; set; }
    }
}
