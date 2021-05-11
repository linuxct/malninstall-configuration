using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using space.linuxct.malninstall.Configuration.Common.Models;

namespace space.linuxct.malninstall.Configuration.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ConfigurationController : ControllerBase
    {
        [HttpGet]
        public MalwarePackagesModel GetPackageNames()
        {
            return new()
            {
                PackageNameList = new List<string>
                {
                    "com.tencent.mm",
                    "com.tencent.mobileqq",
                    "com.clubbing.photos",
                    "com.redtube.music",
                    "com.taobao.taobao",
                    "com.eg.android.AlipayGphone"
                },
                LastUpdateDate = new DateTime(2021, 03, 21, 22, 00, 00)
            };
        }
    }
}