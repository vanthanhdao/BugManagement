using QuanlyBug.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QuanlyBug.Controllers
{
    public class UserController : Controller
    {
        QuanlyBugEntities db = new QuanlyBugEntities();
        // GET: User
        static int UserID = 0;
        public int getUserId()
        {
            return UserID;
        }
        [HttpPost]
        public ActionResult Register(FormCollection collection, USER kh)
        {
            var sFullName = collection["FullName"];
            var sPassword = collection["Password"];
            var sRePassword = collection["RePassword"];
            var sEmail = collection["Email"];
            if (String.IsNullOrEmpty(sFullName))
            {
                ViewData["err1"] = "*Tên không được để trống";
            }
            else if (String.IsNullOrEmpty(sEmail))
            {
                ViewData["err2"] = "*Email không được để trống";
            }
            else if (String.IsNullOrEmpty(sPassword))
            {
                ViewData["err3"] = "*Mật khẩu không được để trống";
            }
            else if (String.IsNullOrEmpty(sRePassword))
            {
                ViewData["err4"] = "*Mật khẩu nhập lại không được để trống";
            }
            else if (db.USERS.SingleOrDefault(n => n.Username == sFullName) != null)
            {
                ViewData["err5"] = "*Tên đăng nhâp đã tồn tại";
            }
            else if (db.USERS.SingleOrDefault(n => n.Email == sEmail) != null)
            {
                ViewData["err6"] = "*Email đã được sử dụng";
            }
            else
            {
                try
                {
                    kh.Username = sFullName;
                    kh.Email = sEmail;
                    kh.Password = sPassword;
                    db.USERS.Add(kh);
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    Console.WriteLine(e);
                }
                return RedirectToAction("Login", "User");
            }
            return RedirectToAction("Register", "User");

        }

        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var sEmail = collection["Email"].ToString();
            var sPassword= collection["Password"].ToString();
            if (String.IsNullOrEmpty(sEmail))
            {
                ViewData["err1"] = "*Email không được để trống";
            }
            else if (String.IsNullOrEmpty(sPassword))
            {
                ViewData["err2"] = "*Mật khẩu không được để trống";
            }
            else
            {
                USER kh = db.USERS.SingleOrDefault(n => n.Email == sEmail && n.Password == sPassword);
                if (kh != null)
                {
                    ViewBag.ThongBao = "*Chúc mừng bạn đăng nhập thành công";
                    Session["Taikhoan"] = kh;
                    UserID = kh.UserID;
                    Session["TaikhoanKH"] = kh.UserID;
                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    ViewBag.ThongBao = "*Tên đăng nhập hoặc mật khẩu không đúng";
                }
            }

            return RedirectToAction("Login","User");
        }
        [ChildActionOnly]
        public ActionResult LoginLogout()
        {
            return PartialView();
        }

    }
}