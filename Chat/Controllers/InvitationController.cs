using Chat.Models;
using Chat.ViewModels;
using System;
using System.Web.Mvc;

namespace Chat.Controllers
{
    [RequireHttps]
    public class InvitationController : Controller
    {
        public ActionResult Index()
        {
            // Check if datetime cookie exists,
            // if it doesn't, serve bucket code to 
            // create cookie on client

            GoogleResponsesViewModel viewModel;
            var cookie = Request.Cookies["safetalk_datetime"];
            if (cookie != null)
            {
               viewModel = GoogleSheetsQueryer.QueryResponses(Uri.UnescapeDataString(cookie.Value));
                ViewBag.refresh = false;
            } else
            {
                viewModel = new GoogleResponsesViewModel();
                ViewBag.refresh = true;
            }

            return View(viewModel);
        }
    }
}