using QuanlyBug.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace QuanlyBug.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        private QuanlyBugDataEntities db = new QuanlyBugDataEntities();

        //public  string CalculateSHA256Hash(string input)
        //{
        //    using (SHA256 sha256 = SHA256.Create())
        //    {
        //        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        //        byte[] hashBytes = sha256.ComputeHash(inputBytes);

        //        StringBuilder sb = new StringBuilder();
        //        foreach (byte b in hashBytes)
        //        {
        //            sb.Append(b.ToString("x2"));
        //        }

        //        return sb.ToString();
        //    }
        //}

        public ActionResult Index()
        {

            return View();
        }

        public ActionResult Project()
        {
            USERS kh = (USERS)Session["TaiKhoan"];
            if (kh != null)
            {
                var data = from pm in db.PROJECTMBS
                           join p in db.PROJECTS on pm.ProjectID equals p.ProjectID
                           join u in db.USERS on pm.UserID equals u.UserID
                           where pm.UserID == kh.UserID
                           select new ProjectList
                           {
                               ProjectID = p.ProjectID,
                               Name = p.Name,
                               Decription = p.Decription,
                               EmailPeoCreate = p.EmailPeoCreate,
                               DateCreate = p.DateCreate,
                               PeopleCreate = p.PeopleCreate,
                               UserID = u.UserID,
                               Username = u.Username,
                               Email = u.Email,
                               Password = u.Password,
                               Role = pm.Role,
                           };
                return View(data.ToList());
            }
            return View();

        }

        [HttpPost]
        public ActionResult CreateProject(PROJECTS project, PROJECTMBS projectmb, FormCollection f)
        {

            USERS kh = (USERS)Session["TaiKhoan"];
            if (kh != null)
            {
                string name = f["NameProject"];
                project.Name = name;
                project.Decription = f["DecriptionProject"];
                project.DateCreate = DateTime.Now.ToString("dd/MM/yyyy H:mm:ss tt");
                project.PeopleCreate = kh.Username.ToString();
                project.EmailPeoCreate = kh.Email.ToString();
                db.PROJECTS.Add(project);
                db.SaveChanges();

                var Project = db.PROJECTS.SingleOrDefault(n => n.Name == name);
                projectmb.ProjectID = Project.ProjectID;
                projectmb.UserID = kh.UserID;
                projectmb.Role = "admin";
                db.PROJECTMBS.Add(projectmb);
                db.SaveChanges();
            }
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

        //[HttpPost]
        //public ActionResult AddMember(FormCollection f)
        //{


        //}

        [HttpPost]
        public ActionResult SendVerificationLinkEmail(FormCollection f)
        {
            string email = f["nameOrEmail"];
            string nameProject = f["nameProject"];
            string role = f["role"];

            var verifyUrl = "/Home/Project/";
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("daothanh1411@gmail.com", "Quản Lý Bug Chain");
            var toEmail = new MailAddress(email);

            if (!email.IsEmpty())
            {
                string subject = "Admin invited you to join them in Jira Software";
                string body = "<!DOCTYPE html>\r\n<html lang=\"en\">\r\n<head>\r\n<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n<style>\r\nbody {\r\n    font-family: Arial, sans-serif;\r\n    background-color: #f4f4f4;\r\n    margin: 0;\r\n    padding: 0;\r\n}\r\n.container {\r\n    background-color: #ffffff;\r\n    width: 100%;\r\n    max-width: 600px;\r\n    margin: 30px auto;\r\n    padding: 20px;\r\n    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n}\r\n.header {\r\n    color: white;\r\n    padding: 10px;\r\n    text-align: center;\r\n}\r\n.header img {\r\n    max-height: 50px;\r\n    width: auto;\r\n}\r\n.content {\r\n    padding: 20px;\r\n    text-align: center;\r\n}\r\n.button {\r\n    display: inline-block;\r\n    margin: 20px 0;\r\n    padding: 10px 20px;\r\n    background-color: #0052CC;\r\n    color: white;\r\n    text-decoration: none;\r\n    border-radius: 5px;\r\n}\r\n.footer {\r\n    text-align: center;\r\n    padding: 10px;\r\n    font-size: 0.8em;\r\n    color: #666;\r\n}\r\n</style>\r\n</head>\r\n<body>\r\n<div class=\"container\">\r\n    <div class=\"header\">\r\n        <img src=\"~/Assets/images/logo.png\" alt=\"Chain App Dev\">\r\n    </div>\r\n    <div class=\"content\">\r\n        <h2>Admin invited you to join them in Jira Software</h2>\r\n        <p>Start planning and tracking work with Admin and your team. You can share your work and view what your team is doing.</p>\r\n        <a href=\"YOUR_INVITATION_LINK\" class=\"button\">Accept Invite</a>\r\n        <p>What is Jira Software? Project and issue tracking <a href=\"YOUR_LEARN_MORE_LINK\">Learn more</a></p>\r\n    </div>\r\n    <div class=\"footer\">\r\n        This message was sent to you by Atlassian Cloud\r\n    </div>\r\n</div>\r\n</body>\r\n</html>";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = true,
                    Credentials = new NetworkCredential(fromEmail.Address, "gfnukgvcrwrcbsqy")
                };
                using (var message = new MailMessage(fromEmail, toEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                    smtp.Send(message);
            }

            return View();

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


        public ActionResult ProjectList()
        {
            return PartialView();
        }

        public ActionResult Team()
        {
            USERS kh = (USERS)Session["TaiKhoan"];
            if (kh != null)
            {
                var data = from pm in db.PROJECTMBS
                           join p in db.PROJECTS on pm.ProjectID equals p.ProjectID
                           join u in db.USERS on pm.UserID equals u.UserID
                           where pm.UserID == kh.UserID
                           select new ProjectList
                           {
                               ProjectID = p.ProjectID,
                               Name = p.Name,
                               Decription = p.Decription,
                               EmailPeoCreate = p.EmailPeoCreate,
                               DateCreate = p.DateCreate,
                               PeopleCreate = p.PeopleCreate,
                               UserID = u.UserID,
                               Username = u.Username,
                               Email = u.Email,
                               Password = u.Password,
                               Role = pm.Role,
                           };
                return PartialView(data.ToList());
            }
            return RedirectToAction("Index", "About");
        }
    }
}