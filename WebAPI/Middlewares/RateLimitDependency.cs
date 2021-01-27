using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Middlewares
{
    public class RateLimitDependency : IRateLimitDependency
    {
        private Dictionary<string, APIRequestIP> _requestIps = new Dictionary<string, APIRequestIP>();

        public APIRequestIP CanRequest(string ip)
        {
            if (!_requestIps.Keys.Contains(ip))
            {
                _requestIps[ip] = new APIRequestIP();
            }
            else
            {
                _requestIps[ip].Count++;

                int count = _requestIps[ip].Count;
                double diff = (DateTime.Now - _requestIps[ip].FirstRequest).TotalMinutes;

                if (diff < 1 && count > 90)
                {
                    _requestIps[ip].Count = 90;
                    _requestIps[ip].RateLimitOK = false;
                }
                else if (diff >= 1)
                {
                    _requestIps[ip].FirstRequest = DateTime.Now;
                    _requestIps[ip].Count = 1;
                    _requestIps[ip].RateLimitOK = true;
                }
            }

            Dictionary<string, APIRequestIP> expired = _requestIps.Where(x => (DateTime.Now - x.Value.FirstRequest).TotalMinutes > 1).ToDictionary(x => x.Key, x => x.Value);

            foreach(string key in expired.Keys)
            {
                _requestIps.Remove(key);
            }

            return _requestIps[ip];
        }
    }
}
