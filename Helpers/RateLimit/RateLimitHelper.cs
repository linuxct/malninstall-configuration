using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using space.linuxct.malninstall.Configuration.Extensions;
using space.linuxct.malninstall.Configuration.Models.Persistence;

namespace space.linuxct.malninstall.Configuration.Helpers.RateLimit
{
    public class RateLimitHelper
    {
        private readonly HttpContext _httpContext;
        private readonly IDistributedCache _distributedCache;
        public RateLimitHelper(HttpContext httpContext, IDistributedCache distributedCache)
        {
            _httpContext = httpContext;
            _distributedCache = distributedCache;
        }
        
        public bool IsClientRateLimited()
        {
            //Get static data from API call via CF
            var connectionIdentifierHash = _httpContext.GetRemoteIPAddress().ToString().ToSha256();
            //Find number of calls from this client in Redis, block if needed
            var numberOfCallsDataModel = _distributedCache.GetObject<NumberOfCallsDataModel>(connectionIdentifierHash) ??
                                         new NumberOfCallsDataModel();
            numberOfCallsDataModel.NumberOfCalls = numberOfCallsDataModel.NumberOfCalls++;
            if (numberOfCallsDataModel.NumberOfCalls >= 10)
            {
                //Has reached rate limit
                //Don't store result in Redis, as it will affect the sliding expiration time
                return true;
            }
            //Store the result in Redis
            _distributedCache.SetTimedObject(connectionIdentifierHash, numberOfCallsDataModel, 1);
            return false;
        }
    }
}