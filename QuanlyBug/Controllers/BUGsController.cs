using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
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
            USERS kh = (USERS)Session["TaiKhoan"];
            var project = db.PROJECTS.SingleOrDefault(p => p.ProjectID == id);
            var users = from pm in db.PROJECTMBS
                        join p in db.PROJECTS on pm.ProjectID equals p.ProjectID
                        join u in db.USERS on pm.UserID equals u.UserID
                        where pm.ProjectID == id 
                        select new UserModel
                        {
                            UserID = (int)pm.UserID,
                            Username = u.Username,
                            Email = u.Email,
                            Status = u.Status,
                        };
            if (kh != null && project != null && users != null)
            {
                ViewData["Project"] = project;
                ViewData["User"] = users.ToList();
                var dataFuctions =
                    from pm in db.PROJECTMBS
                    join p in db.PROJECTS on pm.ProjectID equals p.ProjectID
                    join u in db.USERS on pm.UserID equals u.UserID
                    join f in db.FUNCTIONS on pm.ProjectMembersID equals f.ProjectMembersID
                    where (pm.UserID == kh.UserID && pm.ProjectID == id) 
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
                    join f in db.FUNCTIONS on pm.ProjectMembersID equals f.ProjectMembersID
                    join b in db.BUGS on f.FunctionID equals b.FunctionID
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
                        Reproduce = b.Reproduce,
                        Expected = b.Expected,
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
        public ActionResult UpdateDataBug(int? idFuc, int? idPro)
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
        public ActionResult CreateFunction(FUNCTIONS fuc, PROJECTMBS pm, FormCollection f, int? id)
        {
            USERS kh = (USERS)Session["TaiKhoan"];
            if (kh != null)
            {
                string name = f["NameFunction"];
                string decription = f["DescriptionFunction"];
                string email = f["EmailFunction"];
                var checkName = db.PROJECTS.SingleOrDefault(a => a.Name == name);
                var checkDecription = db.PROJECTS.SingleOrDefault(a => a.Decription == decription);
                if (checkName == null && checkDecription == null)
                {
                    var ProMBs = db.PROJECTMBS.SingleOrDefault(n => n.ProjectID == id && kh.UserID == n.UserID);
                    fuc.Title = name;
                    fuc.ProjectID = id;
                    fuc.Description = decription;
                    fuc.DateCreated = DateTime.Now.ToString("dd/MM/yyyy H:mm:ss tt");
                    fuc.EmailCreater = kh.Email.ToString();
                    fuc.EmailUser = email.ToString();
                    fuc.Status = "Todo";
                    fuc.ProjectMembersID = ProMBs.ProjectMembersID;
                    db.FUNCTIONS.Add(fuc);
                    db.SaveChanges();

                }
            }
            return RedirectToAction("Index", "BUGs", new { id = id });
        }



        [HttpPost]
        public ActionResult CreateBug(BUGS bug, PROJECTMBS pm, FILES file, FormCollection f, HttpPostedFileBase[] files, int? id, int? idFuction)
        {
            USERS kh = (USERS)Session["TaiKhoan"];

            if (kh != null && id != null && idFuction != null)
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
                    var ProMBs = db.PROJECTMBS.SingleOrDefault(n => n.ProjectID == id && n.UserID == kh.UserID);
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
                    bug.ProjectMembersID = ProMBs.ProjectMembersID;
                    db.BUGS.Add(bug);
                    db.SaveChanges();

                    var Bug = db.BUGS.SingleOrDefault(n => n.Title == title);
                  
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
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }


                }
            }
            return RedirectToAction("Index", "BUGs", new { id = id });
        }

        public ActionResult ChangeStatusBug(PROJECTMBS pm, int? idBug, int? idPro, string email = "", string status = "")
        {
            var bug = db.BUGS.SingleOrDefault(b => b.BugID == idBug);
            var user = db.USERS.SingleOrDefault(u => u.Email == email);
            if (bug != null)
            {
                if (user != null)
                {
                    if (status == "Assigned")
                    {
                        //pm.ProjectID = idPro;
                        //pm.UserID = user.UserID;
                        //pm.FunctionID = bug.FunctionID;
                        //pm.Role = "member";
                        //db.PROJECTMBS.Add(pm);
                        //db.SaveChanges();

                        //pm.ProjectID = idPro;
                        //pm.UserID = user.UserID;
                        //pm.FunctionID = bug.FunctionID;
                        //pm.Role = "member";
                        //pm.BugID = idBug;
                        //db.PROJECTMBS.Add(pm);
                        //db.SaveChanges();
                    }
                }
                bug.Status = status;
                db.SaveChanges();
            }
            return RedirectToAction("Index", "BUGs", new { id = idPro });
        }

        [HttpPost]
        public ActionResult EditBug(FILES file,HISTORYS his, FormCollection f, HttpPostedFileBase[] files, int? idPro, int? idBug, int? idFuc)
        {
            USERS kh = (USERS)Session["TaiKhoan"];
            var BUG = db.BUGS.SingleOrDefault(b => b.BugID == idBug);
            var FILE = db.FILES.Where(fi => fi.BugID == idBug);
            var project = db.PROJECTS.SingleOrDefault(p => p.ProjectID == idPro);
            var projectmbs = db.PROJECTMBS.SingleOrDefault(pm => pm.ProjectID == idPro);
            var fuction = db.FUNCTIONS.SingleOrDefault(fi => fi.FunctionID == idFuc);
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
            string status = f["StatusBug"];
            List<string> des = new List<string>();

            if (BUG != null && FILE != null && project != null && projectmbs != null && fuction != null)
            {
                Dictionary<string, (string value, string description)> propertiesToUpdate = new Dictionary<string, (string value, string description)>
                {
                     { "Title",(title, "Tiêu đề") },
                     { "input",(input, "Dữ liệu kiểm tra") },
                     { "Reproduce", (stepsToReproduce,"Các bước tạo lỗi" )},
                     { "Expected", (expected, "Kết quả mong đợi") },
                     { "Actual",(actual, "Kết quả thực tế")},
                     { "url", (url, "Url") },
                     { "severity",( severity, "Mức độ nghiêm trọng")},
                     { "Priority", (priority, "Mức độ ưu tiên") },
                     { "Env",(enviroment, "Môi trường")},
                     { "Description", (description, "Thông tin thêm") },
                     { "Status", (status, "Trạng thái") },
                 };

                foreach (var property in propertiesToUpdate)
                {
                    if (property.Value.value != null && BUG.GetType().GetProperty(property.Key) != null)
                    {
                        string currentValue = BUG.GetType().GetProperty(property.Key).GetValue(BUG, null)?.ToString();
                        if (currentValue != property.Value.value)
                        {
                            BUG.GetType().GetProperty(property.Key).SetValue(BUG, property.Value.value);
                            des.Add(property.Value.description);
                        }
                    }
                }
                if (files != null && files.Length > 0 && files[0]?.ContentLength > 0)
                {
                    des.Add("File đính kèm");
                }

                BUG.Title = title;
                BUG.input = input;
                BUG.Reproduce = stepsToReproduce;
                BUG.Expected = expected;
                BUG.Actual = actual;
                BUG.Env = enviroment;
                BUG.Description = description;
                BUG.url = url;
                BUG.severity = severity;
                BUG.Priority = priority;
                BUG.Status = status;
                db.SaveChanges();

                if (files != null && files.Length > 0 && files[0]?.ContentLength > 0)
                {
                    foreach (var fi in FILE)
                    {
                        db.FILES.Remove(fi);
                    }
                    db.SaveChanges();

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
                            if (fileType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                            {
                                fileType = "application/xlsx";
                            }
                            file.FileType = fileType;
                            file.FileData = fileData;
                            file.BugID = BUG.BugID;

                            db.FILES.Add(file);
                            db.SaveChanges();

                        }
                    }
                }

                string desString = string.Join(", ", des);
                his.ProjectID = projectmbs.ProjectID;
                his.ID_User = kh.UserID;
                his.Activity = "Update";
                his.Time = DateTime.Now.ToString("dd/MM/yyyy H:mm:ss tt");
                his.Name_Project = project.Name;
                his.Description_History = desString+" của bug " + BUG.Title + " được cập nhật trong chức năng " + fuction.Title + " của dự án " + project.Name;
                db.HISTORYS.Add(his);
                db.SaveChanges();


            }
            //try
            //{
            //}
            //catch (DbEntityValidationException ex)
            //{
            //    Lặp qua từng entity validation error để hiển thị thông tin chi tiết
            //foreach (var entityValidationError in ex.EntityValidationErrors)
            //    {
            //        foreach (var validationError in entityValidationError.ValidationErrors)
            //        {
            //            Console.WriteLine($"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}");
            //        }
            //    }
            //}
            return RedirectToAction("Index", "BUGs", new { id = idPro });

        }

        [HttpPost]
        public ActionResult DeleteBug(int? idPro, int? idBug, int? idFuc, FormCollection f, HISTORYS his)
        {
            USERS kh = (USERS)Session["TaiKhoan"];
            var project = db.PROJECTS.SingleOrDefault(p => p.ProjectID == idPro);
            var fuction = db.FUNCTIONS.SingleOrDefault(fi => fi.FunctionID == idFuc);
            var projectmbs = db.PROJECTMBS.SingleOrDefault(pm => pm.ProjectID == idPro );
            var file = db.FILES.Where(fi => fi.BugID == idBug);
            var bug = db.BUGS.SingleOrDefault(b => b.BugID == idBug);
            if (file != null && projectmbs != null && bug != null && kh != null)
            {
                his.ProjectID = projectmbs.ProjectID;
                his.ID_User = kh.UserID;
                his.Activity = "Delete";
                his.Time = DateTime.Now.ToString("dd/MM/yyyy H:mm:ss tt");
                his.Name_Project = project.Name;
                his.Description_History = "Bug " + bug.Title + " được xóa trong chức năng " + fuction.Title + " của dự án " + project.Name;
                db.HISTORYS.Add(his);
                db.SaveChanges();

                db.PROJECTMBS.Remove(projectmbs);
                db.SaveChanges();

                db.FILES.RemoveRange(file);
                db.SaveChanges();

                db.BUGS.Remove(bug);
                db.SaveChanges();
            }
            return RedirectToAction("Index", "BUGs", new { id = idPro });
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
