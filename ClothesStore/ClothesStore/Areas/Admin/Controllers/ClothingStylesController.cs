using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ClothesStore.Library;
using ClothesStore.Models;

namespace ClothesStore.Areas.Admin.Controllers
{
    public class ClothingStylesController : Controller
    {
        private TESTEntities db = new TESTEntities();

        // GET: Admin/ClothingStyles
        public ActionResult Index()
        {
            var clothingStyles = db.ClothingStyles.Include(c => c.ClothingType);
            return View(clothingStyles.Where(clo => clo.IsHidden == false).ToList());
        }

        public ActionResult Trash()
        {
            var clothingStyles = db.ClothingStyles.Include(c => c.ClothingType);
            return View(clothingStyles.Where(clo => clo.IsHidden == true).ToList());
        }

        // GET: Admin/ClothingStyles/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClothingStyle clothingStyle = db.ClothingStyles.Find(id);
            if (clothingStyle == null)
            {
                return HttpNotFound();
            }
            return View(clothingStyle);
        }

        // GET: Admin/ClothingStyles/Create
        public ActionResult Create()
        {
            ViewBag.ClothingTypeID = new SelectList(db.ClothingTypes.Where(clo => clo.IsHidden == false), "ClothingTypeID", "ClothingTypeName");
            return View();
        }

        // POST: Admin/ClothingStyles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ClothingStyle clothingStyle)
        {
            if (ModelState.IsValid)
            {
                if (clothingStyle.ClothingStyleID == null)
                {
                    clothingStyle.ClothingStyleID = "CS000";
                }
                db.ClothingStyles.Add(clothingStyle);
                db.SaveChanges();
                TempData["messages"] = new NotiMessage("success", "Thêm thành công");
                return RedirectToAction("Index");
            }

            ViewBag.ClothingTypeID = new SelectList(db.ClothingTypes.Where(clo => clo.IsHidden == false), "ClothingTypeID", "ClothingTypeName", clothingStyle.ClothingTypeID);
            return View(clothingStyle);
        }

        // GET: Admin/ClothingStyles/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClothingStyle clothingStyle = db.ClothingStyles.Find(id);
            if (clothingStyle == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClothingTypeID = new SelectList(db.ClothingTypes, "ClothingTypeID", "ClothingTypeName", clothingStyle.ClothingTypeID);
            return View(clothingStyle);
        }

        // POST: Admin/ClothingStyles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ClothingStyleID,ClothingStyleName,ClothingTypeID,IsHidden")] ClothingStyle clothingStyle)
        {
            if (ModelState.IsValid)
            {
                db.Entry(clothingStyle).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClothingTypeID = new SelectList(db.ClothingTypes, "ClothingTypeID", "ClothingTypeName", clothingStyle.ClothingTypeID);
            return View(clothingStyle);
        }

        // GET: Admin/ClothingStyles/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClothingStyle clothingStyle = db.ClothingStyles.Find(id);
            if (clothingStyle == null)
            {
                return HttpNotFound();
            }
            return View(clothingStyle);
        }

        // POST: Admin/ClothingStyles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ClothingStyle clothingStyle = db.ClothingStyles.Find(id);
            if (clothingStyle != null)
            {
                clothingStyle.IsHidden = true; // Đánh dấu ẩn
                db.SaveChanges();
            }
            TempData["messages"] = new NotiMessage("danger", "Xóa danh mục thành công");
            return RedirectToAction("Index");
        }
    }
}
