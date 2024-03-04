using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using QuanlyBug.Models;

namespace QuanlyBug.Controllers
{
    public class BUGsController : Controller
    {
        private QuanlyBugEntities db = new QuanlyBugEntities();


        // GET: BUGs
        public ActionResult Index(int? id)
        {
            USER kh = (USER)Session["TaiKhoan"];
            var project = db.PROJECTS.SingleOrDefault(p => p.ProjectID == id);
            var users = from pm in db.PROJECTMBS
                        join p in db.PROJECTS on pm.ProjectID equals p.ProjectID
                        join u in db.USERS on pm.UserID equals u.UserID
                        where pm.ProjectID == id && pm.FunctionID == null && pm.BugID == null
                        select new UserModel
                        {
                            UserID = (int)pm.UserID,
                            Username = u.Username,
                            Email = u.Email,
                            Status = u.Status,
                        };
            if (kh != null && project != null && users != null )
            {
                ViewData["Project"] = project;
                ViewData["User"] = users.ToList();
                var dataFuctions =
                    from pm in db.PROJECTMBS
                    join p in db.PROJECTS on pm.ProjectID equals p.ProjectID
                    join u in db.USERS on pm.UserID equals u.UserID
                    join f in db.FUNCTIONS on pm.FunctionID equals f.FunctionID
                    where (pm.UserID == kh.UserID && pm.ProjectID == id && pm.BugID == null) 
                    select new ProjectList
                    {
                        FunctionID = f.FunctionID,
                        Title = f.Title,
                        EmailCreater = f.EmailCreater,
                        DateCreated = f.DateCreated,
                        DescriptionFunc = f.Description,
                        EmailUser = f.EmailUser,
                        Status = f.Status,
                        ProjectID = (int)pm.ProjectID,
                        UserID = (int)pm.UserID
                    };
                var dataBugs =
                    from pm in db.PROJECTMBS
                    join p in db.PROJECTS on pm.ProjectID equals p.ProjectID
                    join u in db.USERS on pm.UserID equals u.UserID
                    join f in db.FUNCTIONS on pm.FunctionID equals f.FunctionID
                    join b in db.BUGS on pm.BugID equals b.BugID
                    where (pm.UserID == kh.UserID && pm.ProjectID == id )
                    select new ProjectList
                    {
                        BugID = b.BugID,
                        FunctionID = (int)b.FunctionID,
                        TitleBug = b.Title,
                        Description = b.Description,
                        Priority = b.Priority,
                        StatusBug = b.Status,
                        CreatedAt = b.CreatedAt,
                        Severity = b.severity,
                        Url = b.url,
                        Input = b.input,
                        Reproduce =b.Reproduce,
                        Expected =b.Expected ,
                        Actual = b.Actual,
                        Env = b.Env,
                    };
                ViewData["DataFuctions"] = dataFuctions.ToList();
                ViewData["DataBugs"] = dataBugs.ToList();
                
            }
            //HttpContext.Cache["IDProject"] = id;

            return View(db.FILES.ToList());
        }

        [HttpPost]
        public ActionResult UpdateDataBug(int? idFuc,int? idPro)
        {
            Session["idFuction"] = idFuc;
            return Json(new { redirectToAction = Url.Action("Index", "BUGs", new { id = idPro }) });
        }
        public JsonResult GetDataFuncion()
        {
            var data = db.FUNCTIONS.Select(p => new FuctionModel
            {
                FunctionID = p.FunctionID,
                Title = p.Title,
                Description = p.Description
            }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDataBug()
        {
            var data = db.BUGS.Select(p => new BugModel
            {
                BugID = p.BugID,
                Title = p.Title,
            }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDataProjectList(int? id)
        {
            var data = from f in db.FUNCTIONS
                       where f.FunctionID == id
                       select new FuctionModel
                       {
                           FunctionID = f.FunctionID,
                           Title = f.Title,
                           EmailCreater = f.EmailCreater,
                           DateCreated = f.DateCreated,
                           Description = f.Description,
                           EmailUser = f.EmailUser,
                           Status = f.Status,
                           ProjecctID = (int)f.ProjectID
                       };

            return Json(data.SingleOrDefault(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult CreateFunction(FUNCTION fuc,PROJECTMB pm, FormCollection f, int? id)
        {
            USER kh = (USER)Session["TaiKhoan"];
            if (kh != null)
            {
                string name = f["NameFunction"];
                string decription = f["DescriptionFunction"];
                string email = f["EmailFunction"];
                var checkName = db.PROJECTS.SingleOrDefault(a => a.Name == name);
                var checkDecription = db.PROJECTS.SingleOrDefault(a => a.Decription == decription);
                if (checkName == null && checkDecription == null)
                {
                    fuc.Title = name;
                    fuc.ProjectID = id;
                    fuc.Description = decription;
                    fuc.DateCreated = DateTime.Now.ToString("dd/MM/yyyy H:mm:ss tt");
                    fuc.EmailCreater = kh.Email.ToString();
                    fuc.EmailUser = email.ToString();
                    fuc.Status = "Todo";
                    db.FUNCTIONS.Add(fuc);
                    db.SaveChanges();

                    var Fuc = db.FUNCTIONS.SingleOrDefault(n => n.Title == name);
                    pm.ProjectID = Fuc.ProjectID;
                    pm.UserID = kh.UserID;
                    pm.FunctionID = Fuc.FunctionID;
                    pm.Role = "creater";
                    db.PROJECTMBS.Add(pm);
                    db.SaveChanges();

                    var User = db.USERS.SingleOrDefault(n => n.Email == email);
                    pm.ProjectID = Fuc.ProjectID;
                    pm.UserID = User.UserID ;
                    pm.FunctionID = Fuc.FunctionID;
                    pm.Role = "member";
                    db.PROJECTMBS.Add(pm);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index", "BUGs",new {id=id });
        }



        [HttpPost]
        public ActionResult CreateBug(BUG bug, PROJECTMB pm,FILE file, FormCollection f, HttpPostedFileBase[] files, int? id,int? idFuction)
        {
            USER kh = (USER)Session["TaiKhoan"];

            if (kh != null && id!= null && idFuction != null)
            {
                string title = f["title"];
                string input = f["input"];
                string stepsToReproduce = f["stepsToReproduce"];
                string expected = f["expected"];
                string actual = f["actual"];
                string url = f["url"];
                string severity = f["severity"];
                string priority = f["priority"];
                string enviroment = f["enviroment"];
                string description = f["description"];
                var checkName = db.BUGS.SingleOrDefault(a => a.Title == title);
                if (checkName == null)
                {
                    bug.Title = title;
                    bug.Description = description;
                    bug.Priority = priority;
                    bug.Status = "New";
                    bug.CreatedAt = kh.Email;
                    bug.FunctionID = idFuction;
                    bug.severity = severity;
                    bug.url = url;
                    bug.input = input;
                    bug.Reproduce = stepsToReproduce;
                    bug.Expected = expected;
                    bug.Actual = actual;
                    bug.Env = enviroment;
                    db.BUGS.Add(bug);
                    db.SaveChanges();

                    var Bug = db.BUGS.SingleOrDefault(n => n.Title == title);
                    pm.ProjectID = id;
                    pm.UserID = kh.UserID;
                    pm.FunctionID = Bug.FunctionID;
                    pm.Role = "creater";
                    pm.BugID = Bug.BugID;
                    db.PROJECTMBS.Add(pm);
                    db.SaveChanges();
                    try
                    {
                        if (files != null && files.Length > 0)
                        {
                            foreach (var i in files)
                            {
                                if (i != null && i.ContentLength > 0)
                                {
                                    // Lấy thông tin cơ bản của file
                                    var fileName = Path.GetFileName(i.FileName);
                                    var fileType = i.ContentType;

                                    // Đọc dữ liệu của file vào một byte array
                                    byte[] fileData;
                                    using (var binaryReader = new BinaryReader(i.InputStream))
                                    {
                                        fileData = binaryReader.ReadBytes(i.ContentLength);
                                    }
                                    file.FileName = fileName;
                                    file.FileType = fileType;
                                    file.FileData = fileData;
                                    file.BugID = Bug.BugID;

                                    db.FILES.Add(file);
                                    db.SaveChanges();

                                }
                            }
                        }
                    }
                    catch (Exception ex) { 
                        Console.WriteLine(ex.ToString());
                    }
                
                    
                }
            }
            return RedirectToAction("Index", "BUGs", new { id = id });
        }

        public ActionResult ChangeStatusBug(PROJECTMB pm,int? idBug, int? idPro,string email="",string status="")
        {
            var bug = db.BUGS.SingleOrDefault(b => b.BugID == idBug);
            var user = db.USERS.SingleOrDefault(u => u.Email == email);
            if (bug != null)
            {
                if (user != null)
                {
                    if (status == "Assigned")
                    {
                        pm.ProjectID = idPro;
                        pm.UserID = user.UserID;
                        pm.FunctionID = bug.FunctionID;
                        pm.Role = "member";
                        db.PROJECTMBS.Add(pm);
                        db.SaveChanges();

                        pm.ProjectID = idPro;
                        pm.UserID = user.UserID;
                        pm.FunctionID = bug.FunctionID;
                        pm.Role = "member";
                        pm.BugID = idBug;
                        db.PROJECTMBS.Add(pm);
                        db.SaveChanges();
                    }
                }
                bug.Status = status;
                db.SaveChanges();
            }
            return RedirectToAction("Index", "BUGs", new {id=idPro});
        }

        [HttpPost]
        public ActionResult EditBug(int? id, FormCollection f)
        {
            var project = db.PROJECTS.AsEnumerable().SingleOrDefault(n => n.ProjectID == id);
            if (id != null)
            {
                project.Name = f["NameProject"];
                project.Decription = f["DecriptionProject"];
                db.SaveChanges();
                return RedirectToAction("Project", "Home");
            }
            return HttpNotFound();

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
