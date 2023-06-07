using System;
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
            ViewBag.Id = id;
            var bUGS = db.BUGS.Include(b => b.PROJECT).Include(b => b.USER);
            return View(bUGS.ToList());
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
        public ActionResult Create()
        {
           
            ViewBag.ProjectID = new SelectList(db.PROJECTS, "ProjectID", "Name");
            ViewBag.UserID = new SelectList(db.USERS, "UserID", "Username");
            return View();
        }

        // POST: BUGs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        
        public ActionResult Create(BUG bugs, FormCollection f, int? id)
        {
            USER kh = (USER)Session["TaiKhoan"];
            bugs.Title = f["NameBug"];
            bugs.Description = f["description"];
            bugs.Priority = f["Priority"];
            bugs.CreatedAt = DateTime.Now.ToString(); 
            bugs.UpdatedAt = DateTime.Now.ToString();
            bugs.Status = "xác minh";
            bugs.UserID = kh.UserID;
            bugs.ProjectID = id;
            db.BUGS.Add(bugs);
            db.SaveChanges();
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
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BugID,ProjectID,Title,Description,Priority,Status,UserID,CreatedAt,UpdatedAt")] BUG bUG)
        {
            if (ModelState.IsValid)
            {
                db.Entry(bUG).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProjectID = new SelectList(db.PROJECTS, "ProjectID", "Name", bUG.ProjectID);
            ViewBag.UserID = new SelectList(db.USERS, "UserID", "Username", bUG.UserID);
            return View(bUG);
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
