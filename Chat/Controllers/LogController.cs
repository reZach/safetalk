﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Chat.Controllers
{
    [RequireHttps]
    public class LogController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}