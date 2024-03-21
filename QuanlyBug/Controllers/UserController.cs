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
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace QuanlyBug.Controllers
{
    public class UserController : Controller
    {
        QuanlyBugEntities db = new QuanlyBugEntities();
        // GET: User
        public static int UserID = 0;
        static HashSet<string> generatedKeys = new HashSet<string>();

        public ActionResult Account(int? id)
        {
            if (id != null)
            {
                var user = db.USERS.SingleOrDefault(u => u.UserID == id);
                Session["Taikhoan"] = user;
            }
            return View();
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

        [HttpPost]
        public void AuthUser(string email)
        {
            USERS kh = (USERS)Session["Taikhoan"];
            var user = db.USERS.SingleOrDefault(u => u.UserID == kh.UserID);
            if (email != null && user != null)
            {
                string verify = GenerateUniqueRandomKey(6);
                var fromEmail = new MailAddress("daothanh1411@gmail.com", "Quản Lý Bug CChain");
                var toEmail = new MailAddress(email);
                string subject = "Verify Account";
                string body =
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
              "      <p>" + verify + "</p>\r\n     " +
              "  </div>\r\n " +
              "   <div class=\"footer\">\r\n        This message was sent to you by Atlassian Cloud\r\n    </div>" +
              "\r\n</div>" +
              "\r\n</body>" +
              "\r\n</html>";

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
                user.ResetPass = verify;
                db.SaveChanges();
            }
        }

        [HttpPost]
        public ActionResult UpdateAccount(FormCollection f, HttpPostedFileBase file, int? id)
        {
            USERS kh = (USERS)Session["TaiKhoan"];
            var user = db.USERS.SingleOrDefault(u => u.UserID == kh.UserID);
            if (kh != null && user != null)
            {
                string userName = f["newUserName"];
                var checkName = db.USERS.SingleOrDefault(a => a.Username == userName);
                if (checkName == null)
                {
                    try
                    {
                        byte[] fileData = null;
                        if (file != null)
                        {
                            using (var binaryReader = new BinaryReader(file.InputStream))
                            {
                                fileData = binaryReader.ReadBytes(file.ContentLength);
                            }
                        }
                        user.Username = userName;
                        user.Avatar = file != null ? fileData : kh.Avatar;
                        db.SaveChanges();

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            return RedirectToAction("Account", "User", new { id = user.UserID });
        }

        public ActionResult Register()
        {
            var message = TempData["MessageRegister"] as string;
            //string email = Request.QueryString["email"];
            ViewBag.Message = message;
            //ViewBag.email = email;
            return View();
        }

        [HttpPost]
        public ActionResult Register(FormCollection collection, USERS kh)
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
                USERS kh = db.USERS.SingleOrDefault(n => n.Email == sEmail && n.Password == sPassword);
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
            USERS kh = (USERS)Session["Taikhoan"];
            var newPassword = collection["NewPassword"];
            var reNewPassword = collection["ReNewPassword"];
            var authEmail = collection["AuthEmail"];
            var user = db.USERS.SingleOrDefault(u => u.UserID == kh.UserID);
            if (user != null)
            {
                if (newPassword == reNewPassword && authEmail == user.ResetPass)
                {
                    user.Password = newPassword;
                    user.ResetPass = null;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index", "About");

        }

        [ChildActionOnly]
        public ActionResult LoginLogout()
        {
            return PartialView();
        }

        public ActionResult LogOut()
        {
            Session["Taikhoan"] = null;
            return RedirectToAction("Index", "About");
        }

    }
}