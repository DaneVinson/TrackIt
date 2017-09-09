using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TrackIt.Domain.Model.Extensions
{
    public static class HttpStatusCodeExtender
    {
        public static bool IsSuccessResponse(this HttpStatusCode httpStatusCode)
        {
            int value = (int)httpStatusCode;
            return value > 199 && value < 300;
        }
    }
}
