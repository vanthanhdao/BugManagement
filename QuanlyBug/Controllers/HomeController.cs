using QuanlyBug.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

            USER kh = (USER)Session["TaiKhoan"];
            var data = from pmb in db.PROJECTMBS
                       join p in db.PROJECTS on pmb.ProjectID equals p.ProjectID
                       join u in db.USERS on pmb.UserID equals u.UserID
                       //join b in db.BUGS on pmb.BugID equals b.BugID
                       select new GetAllClass
                       {
                           ProjectID = p.ProjectID,
                           ProjectID_PMB = pmb.ProjectID,
                           Name = p.Name,
                           Decription = p.Decription,
                           DateCreate = p.DateCreate,
                           PeopleCreate = p.PeopleCreate,
                           EmailPeoCreate = p.EmailPeoCreate,
                           Role = pmb.Role,
                           Email = u.Email,
                           UserID = u.UserID


                       };



            var user = db.USERS.AsEnumerable().SingleOrDefault(n => n.UserID == kh.UserID);
            var project = db.PROJECTS.ToList();
            //var projectmbs = data.ToList();
            var projectIds = project.Select(p => p.ProjectID).ToList();
            var projectmbs = data.Where(n => projectIds.Contains(n.ProjectID)).ToList();

            ViewBag.projectmbs = projectmbs;
            ViewBag.user = user;
            if (user != null && user.Status == "Employee")
            {
                
                    return View(data.AsEnumerable().SingleOrDefault(n => n.UserID == kh.UserID));

                

                             
            }
            return View(project);


        }

        [HttpPost]
        public ActionResult CreateProject(PROJECT project, FormCollection f)
        {
            USER kh = (USER)Session["TaiKhoan"];
            project.Name = f["NameProject"];
            project.Decription = f["DecriptionProject"];
            project.DateCreate = DateTime.Now.ToString();
            project.PeopleCreate = kh.Username.ToString();
            project.EmailPeoCreate = kh.Email.ToString();
            db.PROJECTS.Add(project);
            db.SaveChanges();
            return RedirectToAction("Project", "Home");
        }


        [HttpPost]
        public ActionResult EditProject(int? id, FormCollection f)
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




        [HttpPost]
        public ActionResult SendMailAddStart(string EmailStart, FormCollection f, PROJECTMB projectmb)
        {
            //USER acc = (USER)Session["TaiKhoan"];
            //string fromEmailAcc = acc.Email.ToString();

            string toemailAcc = EmailStart;
            var fromEmail = new MailAddress("chuhaist123@gmail.com", "CHAIN");
            var toEmail = new MailAddress(toemailAcc);
            string ProjectName = f["projectName"].ToString();
            var project = db.PROJECTS.SingleOrDefault(n => n.Name == ProjectName);
            var user = db.USERS.SingleOrDefault(n => n.Email == EmailStart);


            string subject = "";
            string body = "";
            subject = "Thông Báo";
            body = "<p> Bạn đã được thêm vào dự án " + ProjectName + 
                "<br/>  <strong><a href='https://localhost:44320'>Nhấn vào đây để tham gia</a></strong> " +  
                "</br> <a>  </a> " + "<br/> Xin cảm ơn. <strong></strong>";
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = true,
                Credentials = new NetworkCredential(fromEmail.Address, "gvnggrtvigbpeihd")
            };
            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
                smtp.Send(message);
            projectmb.ProjectID = project.ProjectID;
            projectmb.UserID = user.UserID;
            db.PROJECTMBS.Add(projectmb);
            db.SaveChanges();
            return RedirectToAction("Project", "Home");

        }



    }
}