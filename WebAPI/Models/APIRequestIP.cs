using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class APIRequestIP
    {
        public int Count { get; set; } = 1;
        public DateTime FirstRequest { get; set; } = DateTime.Now;
        public bool RateLimitOK { get; set; } = true;
    }
}
