using QuanlyBug.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace QuanlyBug.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        private QuanlyBugEntities db = new QuanlyBugEntities();

        public ActionResult Index()
        {

            return View();
        }

        public ActionResult Project()
        {

            return View(db.PROJECTS.ToList());
        }

        [HttpPost]
        public ActionResult CreateProject(PROJECT project, FormCollection f )
        {
            USER kh = (USER)Session["TaiKhoan"];
            project.Name = f["NameProject"];
            project.Decription = f["DecriptionProject"];
            project.DateCreate = DateTime.Now.ToString();
            project.PeopleCreate = kh.Username.ToString();
            project.EmailPeoCreate = kh.Email.ToString();
            db.PROJECTS.Add(project);
            db.SaveChanges();
            return RedirectToAction("Project","Home");
        }


        [HttpPost]
        public ActionResult EditProject(int? id,FormCollection f)
        {
            var project = db.PROJECTS.AsEnumerable().SingleOrDefault(n => n.ProjectID == id);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (project == null)
            {
                return HttpNotFound();
            }
            else
            {
                project.Name = f["NameProject"];
                project.Decription = f["DecriptionProject"];
                db.SaveChanges();
                return RedirectToAction("Project", "Home");
            }

        }

        [HttpPost]
        public ActionResult DeleteProject(int? id, FormCollection f)
        {
            var project = db.PROJECTS.SingleOrDefault(n => n.ProjectID == id);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                db.PROJECTS.Remove(project);
                db.SaveChanges();
                return RedirectToAction("Project", "Home");

            }

        }


    }
}