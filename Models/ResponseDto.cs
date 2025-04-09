using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperPlayHA.Models
{
    public class ResponseDto
    {
        public string Status { get; set; } = string.Empty;
        public string PlayerId { get; set; } = string.Empty;
        public int NewBalance { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
