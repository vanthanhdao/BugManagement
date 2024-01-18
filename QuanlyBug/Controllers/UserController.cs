using QuanlyBug.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;

namespace QuanlyBug.Controllers
{
    public class UserController : Controller
    {
        QuanlyBugEntities db = new QuanlyBugEntities();
        // GET: User
        public static int UserID = 0;

        public ActionResult Register()
        {
            var message = TempData["MessageRegister"] as string;
            ViewBag.Message = message;
            return View();
        }

        [HttpPost]
        public ActionResult Register(FormCollection collection, USER kh)
        {
            var sFullName = collection["FullName"];
            var sPassword = collection["Password"];
            var sEmail = collection["Email"];
            var user = db.USERS.ToList();
            if (!String.IsNullOrEmpty(sFullName) && !String.IsNullOrEmpty(sPassword) && !String.IsNullOrEmpty(sEmail))
            {
                try
                {
                    foreach (var i in user)
                    {
                        if (i.Username == sFullName)
                        {
                            TempData["MessageRegister"] = "Họ và tên đã được sử dụng";
                            break;
                        }
                        else if (i.Email == sEmail)
                        {
                            TempData["MessageRegister"] = "Email đã được sử dụng";
                            break;
                        }
                        else if (i.Password == sPassword)
                        {
                            TempData["MessageRegister"] = "Password đã được sử dụng";
                            break;
                        }
                        else
                        {
                            kh.Username = sFullName;
                            kh.Email = sEmail;
                            kh.Password = sPassword;
                            db.USERS.Add(kh);
                            db.SaveChanges();
                            return RedirectToAction("Index", "About");
                        }
                    }

                }
                catch (DbEntityValidationException e)
                {
                    Console.WriteLine(e);
                }
            }
            return RedirectToAction("Register", "User");

        }

        [HttpPost]
        public ActionResult Login(FormCollection collection)
        {
            var sEmail = collection["Email"].ToString();
            var sPassword = collection["Password"].ToString();
            var messagelogin = "";
            if (!String.IsNullOrEmpty(sEmail) && !String.IsNullOrEmpty(sPassword))
            {
                USER kh = db.USERS.SingleOrDefault(n => n.Email == sEmail && n.Password == sPassword);
                if (kh != null)
                {
                    Session["Taikhoan"] = kh;
                    UserID = kh.UserID;
                    Session["TaikhoanKH"] = kh.UserID;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    messagelogin = "Tên đăng nhập hoặc mật khẩu không đúng";
                    TempData["Messagelogin"] = messagelogin;
                }
            }
            return RedirectToAction("Index", "About");
        }


        public ActionResult ForgotPassword()
        {
            var message = TempData["Message"] as string;
            ViewBag.Message = message;
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string Email)
        {

            string message = "";
            var account = db.USERS.Where(a => a.Email == Email).FirstOrDefault();
            if (account != null)
            {
                string resetCode = Guid.NewGuid().ToString();
                SendVerificationLinkEmail(account.Email, resetCode, "ResetPassword");
                UserID = account.UserID;
                account.ResetPass = resetCode;
                db.SaveChanges();
                message = "Liên kết khôi phục mật khẩu đã được gửi đến email của bạn <3";
            }
            else
            {
                message = "Không tìm thấy email";
            }
            TempData["Message"] = message;
            return RedirectToAction("ForgotPassword", "User");
        }

        [NonAction]
        public void SendVerificationLinkEmail(string email, string activationCode, string emailFor = "VerifyAccount")
        {
            var verifyUrl = "/User/" + emailFor + "/";
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("daothanh1411@gmail.com", "Quản Lý Bug Chain");
            var toEmail = new MailAddress(email);

            string subject = "";
            string body = "";

            if (emailFor == "VerifyAccount")
            {
                subject = "Your account is successfully created";
                body = "<br /><br /> We are  <a href='" + link + "'>" + link + "</a>";
            }
            else if (emailFor == "ResetPassword")
            {
                subject = "Reset Password";
                body = "Chúng tôi xác nhận tài khoản này thuộc về bạn, vui lòng ấn vào <a href='" + link + "'> Khôi phục mật khẩu </a>";
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

        public ActionResult ResetPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ResetPassword(FormCollection collection)
        {
            var demo = UserID;
            var sNewPassword = collection["NewPassword"];

            var user = db.USERS.Where(a => a.UserID == demo).FirstOrDefault();
            if (user != null)
            {
                user.Password = sNewPassword;
                user.ResetPass = " ";
                db.Configuration.ValidateOnSaveEnabled = false;
                db.SaveChanges();
                return RedirectToAction("Login", "User");
            }
            return RedirectToAction("ResetPassword", "User");

        }


        [ChildActionOnly]
        public ActionResult LoginLogout()
        {
            return PartialView();
        }

    }
}