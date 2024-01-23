using QuanlyBug.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanlyBug.Controllers
{
    public class AboutController : Controller
    {
        // GET: About
        private QuanlyBugDataEntities db = new QuanlyBugDataEntities();

        public ActionResult Index()
        {
            var message = TempData["Messagelogin"] as string;
            ViewBag.Message = message;
            return View();
        }

        public ActionResult Test_Data()
        {
            return View();
        }
    }
}