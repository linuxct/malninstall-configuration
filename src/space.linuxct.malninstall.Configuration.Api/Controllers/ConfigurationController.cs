using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using space.linuxct.malninstall.Configuration.ViewModels.Configuration;

namespace space.linuxct.malninstall.Configuration.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ConfigurationController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetPackageNames()
        {
            return Ok(new MalwarePackagesViewModel()
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
            });
        }
    }
}