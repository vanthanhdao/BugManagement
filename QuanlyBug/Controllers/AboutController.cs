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
        private QuanlyBugEntitiess db = new QuanlyBugEntitiess();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Test_Data()
        {
            return View();
        }
    }
}