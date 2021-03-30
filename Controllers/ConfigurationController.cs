using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace space.linuxct.malninstall.Configuration.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ConfigurationController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<string> GetPackageNames()
        {
            return new List<string>
            {
                "com.tencent.mm",
                "com.tencent.mobileqq",
                "com.clubbing.photos",
                "com.redtube.music",
                "com.taobao.taobao",
                "com.eg.android.AlipayGphone"
            };
        }
    }
}