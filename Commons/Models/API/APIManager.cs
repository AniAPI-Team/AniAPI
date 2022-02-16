using Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Commons
{
    public static class APIManager
    {
        public static APIResponse SuccessResponse(string message = "", object data = null, string version = "1")
        {
            return new APIResponse()
            {
                StatusCode = HttpStatusCode.OK,
                Message = message,
                Data = data,
                Version = version
            };
        }

        public static APIResponse ErrorResponse(APIException ex)
        {
            return new APIResponse()
            {
                StatusCode = ex.StatusCode,
                Message = ex.Message,
                Data = ex.Description,
                Version = ex.Version
            };
        }

        public static APIResponse ErrorResponse()
        {
            return new APIResponse();
        }
    }
}
