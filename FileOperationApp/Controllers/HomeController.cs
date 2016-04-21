using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FileOperationApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.message = "Your contact page.";

            return View();
        }
    }
}