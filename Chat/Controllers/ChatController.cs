using Chat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Chat.Controllers
{
    [RequireHttps]
    public class ChatController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public void ClearCache()
        {
            if (!string.IsNullOrEmpty(Request.QueryString["a"]))
            {
                if (Request.QueryString["a"] == WebConfigurationManager.AppSettings["RefreshCacheSecret"])
                {
                    RedisQueryer.ClearCache();
                }
            }
        }
    }
}