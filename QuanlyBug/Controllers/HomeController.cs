using Microsoft.Ajax.Utilities;
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
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.WebPages;

namespace QuanlyBug.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        private QuanlyBugEntities db = new QuanlyBugEntities();
        static HashSet<string> generatedKeys = new HashSet<string>();


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

            string email = Request.QueryString["email"];
            if (!email.IsEmpty())
            {
                USERS khs = db.USERS.SingleOrDefault(n => n.Email == email);
                Session["Taikhoan"] = khs;
            }

            USERS kh = (USERS)Session["TaiKhoan"];
            if (kh != null)
            {

                var data = kh.Status == "admin" ?
                    (from p in db.PROJECTS
                     join b in db.BUGS on p.ProjectID equals b.ProjectID into bs
                     join f in db.FUNCTIONS on p.ProjectID equals f.ProjectID into fs
                     //Lỗi Bug k truy suất dk
                     select new ProjectList
                     {
                         ProjectID = p.ProjectID,
                         Name = p.Name,
                         Decription = p.Decription,
                         EmailPeoCreate = p.EmailPeoCreate,
                         DateCreate = p.DateCreate,
                         PeopleCreate = p.PeopleCreate,
                         countFunctions = fs.Count(),
                         countBugs = bs.Count(),
                         countFunctionsNew = fs.Where(f => f.Status == "Todo").Count(),
                         countBugsNew = bs.Where(b => b.Status == "New").Count(),
                     })
                     : (from pm in db.PROJECTMBS
                        join p in db.PROJECTS on pm.ProjectID equals p.ProjectID
                        join b in db.BUGS on pm.ProjectID equals b.ProjectID into bs
                        join f in db.FUNCTIONS on pm.ProjectID equals f.ProjectID into fs
                        join u in db.USERS on pm.UserID equals u.UserID
                        where pm.UserID == kh.UserID
                        //Lỗi Bug k truy suất dk
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
                            countFunctions = fs.Count(),
                            countBugs = bs.Count(),
                            countFunctionsNew = fs.Where(f => f.Status == "Todo").Count(),
                            countBugsNew = bs.Where(b => b.Status == "New").Count(),
                        });
                var dataHistory = kh.Status == "admin" ?
                     (from h in db.HISTORYS
                      join u in db.USERS on h.ID_User equals u.UserID
                      //Lỗi Bug k truy suất dk
                      select new HistoryList
                      {
                          ID_History = h.ID_History,
                          Name_Project = h.Name_Project,
                          Description_History = h.Description_History,
                          Time = h.Time,
                          Activity = h.Activity,
                          nameUser = u.Email,
                          ID_User = h.ID_User
                      }).ToList()
                      : (from h in db.HISTORYS
                         join pm in db.PROJECTMBS on kh.UserID equals pm.UserID
                         join p in db.PROJECTS on pm.ProjectID equals p.ProjectID
                         join u in db.USERS on h.ID_User equals u.UserID
                         where h.ProjectID == p.ProjectID
                         //Lỗi Bug k truy suất dk
                         select new HistoryList
                         {
                             ID_History = h.ID_History,
                             Name_Project = h.Name_Project,
                             Description_History = h.Description_History,
                             Time = h.Time,
                             Activity = h.Activity,
                             nameUser = u.Email,
                             ID_User = h.ID_User
                         }).ToList();

                // xử lí người nào trong chức năng nào chỉ coi được history đó
                ViewData["dataHistory"] = dataHistory;
                //var coutFunctionshas = (from pm in db.PROJECTMBS
                //                        join f in db.FUNCTIONS on pm.ProjectID equals f.ProjectID
                //                        join p in db.PROJECTS on pm.ProjectID equals p.ProjectID into g
                //                        join u in db.USERS on pm.UserID equals u.UserID
                //                        where pm.UserID == kh.UserID
                //                        sel).ToList();
                ////lấy cái coutn function bugs
                //var countFunctions = from pm in db.PROJECTMBS
                //           join p in db.PROJECTS on pm.ProjectID equals p.ProjectID
                //           join u in db.USERS on pm.UserID equals u.UserID
                //           where pm.UserID == kh.UserID && pm.FunctionID == null
                //           select new ProjectList
                //           {
                //               ProjectID = p.ProjectID,

                //           };,,,,,,,,,,,,,,,,,,,,,,,
                //ViewData["countFunctionhas"] = coutFunctionshas;
                ViewData["ProjectList"] = data.ToList();
                return View(data.ToList());
            }
            return View();
        }

        public JsonResult GetData()
        {
            var data = db.USERS.Select(p => new UserModel
            {
                UserID = p.UserID,
                Username = p.Username,
                Email = p.Email,
                Status = p.Status
            }).ToList();
            // Trả về dữ liệu dưới dạng JSON
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDataProjectList()
        {
            USERS kh = (USERS)Session["TaiKhoan"];
            var data = kh != null && kh.Status == "admin" ?
                from p in db.PROJECTS
                select new ProjectMemberModel
                {
                    Name = p.Name,
                    ProjectID = p.ProjectID,
                }
                :
                (from pm in db.PROJECTMBS
                 join p in db.PROJECTS on pm.ProjectID equals p.ProjectID
                 join u in db.USERS on pm.UserID equals u.UserID
                 select new ProjectMemberModel
                 {
                     UserID = pm.UserID,
                     Username = u.Username,
                     Email = u.Email,
                     Name = p.Name,
                     Status = u.Status,
                     ProjectID = pm.ProjectID,
                     Role = pm.Role,
                 });
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetDataProject()
        {

            var data = db.PROJECTS.Select(p => new ProjectModel
            {
                ProjectID = p.ProjectID,
                Name = p.Name,
                Decription = p.Decription,
                EmailPeoCreate = p.EmailPeoCreate
            }).ToList();
            // Trả về dữ liệu dưới dạng JSON
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Project()
        {
            string email = Request.QueryString["email"];
            if (!email.IsEmpty())
            {
                USERS user = db.USERS.FirstOrDefault(n => n.Email == email);
                Session["Taikhoan"] = user;
            }
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
        public ActionResult CreateProject(PROJECTS project, PROJECTMBS projectmb, HISTORYS his, FormCollection f)
        {

            USERS kh = (USERS)Session["TaiKhoan"];
            if (kh != null)
            {
                string name = f["NameProject"];
                string decription = f["DecriptionProject"];
                var checkName = db.PROJECTS.SingleOrDefault(a => a.Name == name);
                var checkDecription = db.PROJECTS.SingleOrDefault(a => a.Decription == decription);
                if (checkName == null && checkDecription == null)
                {
                    project.Name = name;
                    project.Decription = decription;
                    project.DateCreate = DateTime.Now.ToString("dd/MM/yyyy H:mm:ss tt");
                    project.PeopleCreate = kh.Username.ToString();
                    project.EmailPeoCreate = kh.Email.ToString();
                    db.PROJECTS.Add(project);
                    db.SaveChanges();

                    var Project = db.PROJECTS.SingleOrDefault(n => n.Name == name);
                    projectmb.ProjectID = Project.ProjectID;
                    projectmb.UserID = kh.UserID;
                    projectmb.Role = "creater";
                    db.PROJECTMBS.Add(projectmb);
                    db.SaveChanges();

                    his.ProjectID = projectmb.ProjectID;
                    his.Name_Project = Project.Name;
                    his.Description_History = kh.Email.ToString() + " đã tạo dự án " + Project.Name;
                    his.Time = DateTime.Now.ToString("dd/MM/yyyy H:mm:ss tt"); ;
                    his.Activity = "Create";
                    his.ID_User = kh.UserID;
                    db.HISTORYS.Add(his);
                    db.SaveChanges();
                }

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

        static string GenerateUniqueRandomKey(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            StringBuilder randomKey = new StringBuilder(length);

            while (true)
            {
                for (int i = 0; i < length; i++)
                {
                    int index = random.Next(chars.Length);
                    randomKey.Append(chars[index]);
                }

                string key = randomKey.ToString();

                if (!generatedKeys.Contains(key))
                {
                    generatedKeys.Add(key);
                    return key;
                }

                // Clear StringBuilder for the next iteration if the key is not unique
                randomKey.Clear();
            }
        }

        public void SendVerifyEmail(string verifyUrl, string email, string target = "author")
        {
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            var fromEmail = new MailAddress("daothanh1411@gmail.com", "Quản Lý Bug CChain");
            var toEmail = new MailAddress(email);
            string subject = "Admin invited you to join them in CCHAIN";
            string body = "";
            if (target == "author")
            {
                body =
          "<!DOCTYPE html>\r\n" +
          "<html lang=\"en\">\r\n" +
          "<head>\r\n" +
          "<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
          "<style>\r\nbody {\r\n    font-family: Arial, sans-serif;\r\n    background-color: #f4f4f4;\r\n    margin: 0;\r\n    padding: 0;\r\n}\r\n.container {\r\n    background-color: #ffffff;\r\n    width: 100%;\r\n    max-width: 600px;\r\n    margin: 30px auto;\r\n    padding: 20px;\r\n    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n}\r\n.header {\r\n    color: white;\r\n    padding: 10px;\r\n    text-align: center;\r\n}\r\n.header img {\r\n    max-height: 50px;\r\n    width: auto;\r\n}\r\n.content {\r\n    padding: 20px;\r\n    text-align: center;\r\n}\r\n.button {\r\n    display: inline-block;\r\n    margin: 20px 0;\r\n    padding: 10px 20px;\r\n    background-color: #0052CC;\r\n    color: white;\r\n    text-decoration: none;\r\n    border-radius: 5px;\r\n}\r\n.footer {\r\n    text-align: center;\r\n    padding: 10px;\r\n    font-size: 0.8em;\r\n    color: #666;\r\n}\r\n</style>" +
          "\r\n</head>" +
          "\r\n" +
          "<body>\r\n" +
          "<div class=\"container\">\r\n  " +
          "  <div class=\"header\">\r\n      " +
          "  <img src=\"https://firebasestorage.googleapis.com/v0/b/fir-d9bb1.appspot.com/o/logo.png?alt=media&token=1eb8f3b7-fc35-4bb6-a80b-3abae6ff651f\" alt=\"Chain App Dev\">\r\n   " +
          " </div>\r\n   " +
          " <div class=\"content\">\r\n    " +
          "    <h2>Admin invited you to join them in Jira Software</h2>\r\n    " +
          "    <p>Start planning and tracking work with Admin and your team. You can share your work and view what your team is doing.</p>\r\n  " +
          "      <a href='" + link + "' class=\"button\" style=\"color:white\">Accept Invite</a>\r\n     " +
          "   <p>What is Jira Software? Project and issue tracking <a href=\"YOUR_LEARN_MORE_LINK\">Learn more</a></p>\r\n  " +
          "  </div>\r\n " +
          "   <div class=\"footer\">\r\n        This message was sent to you by Atlassian Cloud\r\n    </div>" +
          "\r\n</div>" +
          "\r\n</body>" +
          "\r\n</html>";
            }
            else
            {
                body =
         "<!DOCTYPE html>\r\n" +
         "<html lang=\"en\">\r\n" +
         "<head>\r\n" +
         "<meta charset=\"UTF-8\">\r\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
         "<style>\r\nbody {\r\n    font-family: Arial, sans-serif;\r\n    background-color: #f4f4f4;\r\n    margin: 0;\r\n    padding: 0;\r\n}\r\n.container {\r\n    background-color: #ffffff;\r\n    width: 100%;\r\n    max-width: 600px;\r\n    margin: 30px auto;\r\n    padding: 20px;\r\n    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);\r\n}\r\n.header {\r\n    color: white;\r\n    padding: 10px;\r\n    text-align: center;\r\n}\r\n.header img {\r\n    max-height: 50px;\r\n    width: auto;\r\n}\r\n.content {\r\n    padding: 20px;\r\n    text-align: center;\r\n}\r\n.button {\r\n    display: inline-block;\r\n    margin: 20px 0;\r\n    padding: 10px 20px;\r\n    background-color: #0052CC;\r\n    color: white;\r\n    text-decoration: none;\r\n    border-radius: 5px;\r\n}\r\n.footer {\r\n    text-align: center;\r\n    padding: 10px;\r\n    font-size: 0.8em;\r\n    color: #666;\r\n}\r\n</style>" +
         "\r\n</head>" +
         "\r\n" +
         "<body>\r\n" +
         "<div class=\"container\">\r\n  " +
         "  <div class=\"header\">\r\n      " +
         "  <img src=\"https://firebasestorage.googleapis.com/v0/b/fir-d9bb1.appspot.com/o/logo.png?alt=media&token=1eb8f3b7-fc35-4bb6-a80b-3abae6ff651f\" alt=\"Chain App Dev\">\r\n   " +
         " </div>\r\n   " +
         " <div class=\"content\">\r\n    " +
         "    <h2>Admin invited you to join them in Jira Software</h2>\r\n    " +
         "    <p>Start planning and tracking work with Admin and your team. You can share your work and view what your team is doing.</p>\r\n  " +
         "      <a href='" + link + "' class=\"button\" style=\"color:white\">Accept Invite</a>\r\n     " +
         "   <p>What is Jira Software? Project and issue tracking <a href=\"YOUR_LEARN_MORE_LINK\">Learn more</a></p>\r\n  " +
         "  </div>\r\n " +
         "   <div class=\"footer\">\r\n        This message was sent to you by Atlassian Cloud\r\n    </div>" +
         "\r\n</div>" +
         "\r\n</body>" +
         "\r\n</html>";
            }


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

        [HttpPost]
        public void Authorization(string email, string role)
        {
            USERS kh = new USERS();
            var user = db.USERS.AsEnumerable().SingleOrDefault(n => n.Email == email);
            var verifyUrl = "#";
            if (user == null && !email.IsEmpty())
            {
                //verifyUrl = "/User/Register?email=" + email + "and&role=" + role;
                verifyUrl = "/Home/Index?email=" + email;
                kh.Username = GenerateUniqueRandomKey(12);
                kh.Email = email;
                kh.Password = GenerateUniqueRandomKey(12);
                kh.Status = role;
                db.USERS.Add(kh);
                db.SaveChanges();
                SendVerifyEmail(verifyUrl, email);
            }
            else if (user.Status == null)
            {
                verifyUrl = "/About/Index/";
                user.Status = role;
                db.SaveChanges();
                SendVerifyEmail(verifyUrl, email);
            }

        }

        [HttpPost]
        public void AuthorizationAddMember(string email, string nameProject)
        {
            PROJECTMBS pmb = new PROJECTMBS();
            var user = db.USERS.FirstOrDefault(n => n.Email == email);
            var project = db.PROJECTS.FirstOrDefault(n => n.Name == nameProject);
            var projectmbs = db.PROJECTMBS.ToList();
            var verifyUrl = "#";
            if (pmb != null && user != null && project != null)
            {
                bool exists = projectmbs.Any(e => e.ProjectID == project.ProjectID && e.UserID == user.UserID);
                if (!exists)
                {
                    verifyUrl = "/Home/Project?email=" + email;
                    pmb.ProjectID = project.ProjectID;
                    pmb.UserID = user.UserID;
                    pmb.Role = "member";
                    db.PROJECTMBS.Add(pmb);
                    db.SaveChanges();
                    SendVerifyEmail(verifyUrl, email, "addmember");
                }
            }
        }



        [HttpPost]
        public ActionResult DeleteProject(int? id, FormCollection f)
        {
            var project = db.PROJECTS.SingleOrDefault(n => n.ProjectID == id);
            var projectmbs = db.PROJECTMBS.Where(pm => pm.ProjectID == id);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                db.PROJECTMBS.RemoveRange(projectmbs);
                db.SaveChanges();

                db.PROJECTS.Remove(project);
                db.SaveChanges();
                return RedirectToAction("Project", "Home");
            }

        }

        public ActionResult Team()
        {
            USERS kh = (USERS)Session["TaiKhoan"];
            if (kh != null)
            {
                var data = kh.Status == "admin" ?
                    (from p in db.PROJECTS
                     join f in db.FUNCTIONS on p.ProjectID equals f.ProjectID into fs
                     select new ProjectList
                     {
                         ProjectID = p.ProjectID,
                         Name = p.Name,
                         Decription = p.Decription,
                         EmailPeoCreate = p.EmailPeoCreate,
                         DateCreate = p.DateCreate,
                         PeopleCreate = p.PeopleCreate,

                     }) :
                (from pm in db.PROJECTMBS
                 join p in db.PROJECTS on pm.ProjectID equals p.ProjectID
                 join f in db.FUNCTIONS on pm.ProjectID equals f.ProjectID into fs
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
                 });
                return PartialView(data.ToList());
            }
            return RedirectToAction("Index", "About");
        }

    }
}