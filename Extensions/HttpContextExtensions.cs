using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace space.linuxct.malninstall.Configuration.Extensions
{
    public static class HttpContextExtensions
    {
        public static IPAddress GetRemoteIPAddress(this HttpContext context)
        {
            var header = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? context.Request.Headers["Cf-Connecting-Ip"].FirstOrDefault() ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            return IPAddress.TryParse(header, out IPAddress ip) ? ip : context.Connection.RemoteIpAddress;
        }
    }

}