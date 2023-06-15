﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
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
            //var bugs = db.PROJECTS.AsEnumerable().SingleOrDefault(n => n.ProjectID == id);
            //if (id != null)
            //{
            //    ViewBag.IdPr = bugs.ProjectID.ToString();
            //    Session["IdProject"] = bugs;
            //}
            //var bUGS = db.BUGS.Include(b => b.PROJECT).Include(b => b.USER);
            //return View(bUGS.ToList());

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
            var bug = db.BUGS.ToList();
            //var projectmbs = data.ToList();
            var projectIds = project.Select(p => p.ProjectID).ToList();
            var projectmbs = data.Where(n => projectIds.Contains(n.ProjectID)).ToList();

            ViewBag.projectmbs = projectmbs;
            ViewBag.project = project;
            ViewBag.user = user;
            ViewBag.id = id;
            if (user != null && user.Status == "Employee")
            {
                return View(data.AsEnumerable().SingleOrDefault(n => n.UserID == kh.UserID));
            }
            return View(bug);

        }

        // GET: BUGs/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BUG bUG = db.BUGS.Find(id);
            if (bUG == null)
            {
                return HttpNotFound();
            }
            return View(bUG);
        }
        int GetID;

        // GET: BUGs/Create
        //public ActionResult Create()
        //{


        //    ViewBag.ProjectID = new SelectList(db.PROJECTS, "ProjectID", "Name");
        //    ViewBag.UserID = new SelectList(db.USERS, "UserID", "Username");
        //    return View();
        //}

        // POST: BUGs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        public ActionResult Create(BUG bugs,PROJECTMB projectmbs, FormCollection f, int? id)
        {
            USER kh = (USER)Session["TaiKhoan"];
            bugs.Title = f["NameBug"];
            bugs.Description = f["description"];
            bugs.Priority = f["Priority"];
            bugs.CreatedAt = DateTime.Now.ToString();
            //bugs.UpdatedAt = DateTime.Now.ToString();
            bugs.Status = "xác minh";
            bugs.UserID = kh.UserID;
            bugs.ProjectID = id;
            db.BUGS.Add(bugs);
            //projectmbs.Role = f["Role"];
            //db.PROJECTMBS.Add(projectmbs);
            db.SaveChanges();
            ViewBag.IdPr = id;
            return RedirectToAction("Index", "BUGs");
        }

        // GET: BUGs/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BUG bUG = db.BUGS.Find(id);
            if (bUG == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProjectID = new SelectList(db.PROJECTS, "ProjectID", "Name", bUG.ProjectID);
            ViewBag.UserID = new SelectList(db.USERS, "UserID", "Username", bUG.UserID);
            return View(bUG);
        }

        // POST: BUGs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Edit(FormCollection f, int? id, int? idpro)
        {
            var bugs = db.BUGS.AsEnumerable().SingleOrDefault(n => n.BugID == id);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (bugs == null)
            {
                return HttpNotFound();
            }
            else
            {
                bugs.Title = f["NameBug"];
                bugs.Description = f["description"];
                bugs.UpdatedAt = DateTime.Now.ToString();
                if (bugs.Status == "xác minh") bugs.Status = "Đang giải quyết";
                else if (bugs.Status == "Đang giải quyết") bugs.Status = "Xác thực";
                else if (bugs.Status == "Xác thực") bugs.Status = "Đã xong";
                db.SaveChanges();
                ViewBag.IdPr = idpro;
                return RedirectToAction("Index", "BUGs");
            }
        }
        public ActionResult Editback(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BUG bUG = db.BUGS.Find(id);
            if (bUG == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProjectID = new SelectList(db.PROJECTS, "ProjectID", "Name", bUG.ProjectID);
            ViewBag.UserID = new SelectList(db.USERS, "UserID", "Username", bUG.UserID);
            return View(bUG);
        }

        // POST: BUGs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult Editback(FormCollection f, int? id, int? idpro)
        {
            var bugs = db.BUGS.AsEnumerable().SingleOrDefault(n => n.BugID == id);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (bugs == null)
            {
                return HttpNotFound();
            }
            else
            {
                bugs.Title = f["NameBug"];
                bugs.Description = f["description"];
                bugs.UpdatedAt = DateTime.Now.ToString();
                if (bugs.Status == "Đã xong") bugs.Status = "Xác thực";
                else if (bugs.Status == "Xác thực") bugs.Status = "Đang giải quyết";
                else if (bugs.Status == "Đang giải quyết") bugs.Status = "xác minh";
                db.SaveChanges();
                ViewBag.IdPr = idpro;
                return RedirectToAction("Index", "BUGs");
            }
        }
        // GET: BUGs/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BUG bUG = db.BUGS.Find(id);
            if (bUG == null)
            {
                return HttpNotFound();
            }
            return View(bUG);
        }

        // POST: BUGs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            BUG bUG = db.BUGS.Find(id);
            db.BUGS.Remove(bUG);
            db.SaveChanges();
            return RedirectToAction("Index");
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
