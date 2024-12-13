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
    public class ClothingTypesController : Controller
    {
        private TESTEntities db = new TESTEntities();

        // GET: Admin/ClothingTypes
        public ActionResult Index()
        {
            return View(db.ClothingTypes.Where(clo => clo.IsHidden == false).ToList());
        }

        public ActionResult Trash()
        {
            return View(db.ClothingTypes.Where(clo => clo.IsHidden == true).ToList());
        }

        // GET: Admin/ClothingTypes/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClothingType clothingType = db.ClothingTypes.Find(id);
            if (clothingType == null)
            {
                return HttpNotFound();
            }
            return View(clothingType);
        }

        // GET: Admin/ClothingTypes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/ClothingTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ClothingTypeID,ClothingTypeName,IsHidden")] ClothingType clothingType)
        {
            if (ModelState.IsValid)
            {
                if (clothingType.ClothingTypeID == null)
                {
                    clothingType.ClothingTypeID = "CT000";
                }
                db.ClothingTypes.Add(clothingType);
                db.SaveChanges();
                TempData["messages"] = new NotiMessage("success", "Thêm thành công");
                return RedirectToAction("Index");
            }

            return View(clothingType);
        }

        // GET: Admin/ClothingTypes/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClothingType clothingType = db.ClothingTypes.Find(id);
            if (clothingType == null)
            {
                return HttpNotFound();
            }
            return View(clothingType);
        }

        // POST: Admin/ClothingTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ClothingTypeID,ClothingTypeName,IsHidden")] ClothingType clothingType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(clothingType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(clothingType);
        }

        // GET: Admin/ClothingTypes/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClothingType clothingType = db.ClothingTypes.Find(id);
            if (clothingType == null)
            {
                return HttpNotFound();
            }
            return View(clothingType);
        }

        // POST: Admin/ClothingTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ClothingType clothingType = db.ClothingTypes.Find(id);
            if (clothingType != null)
            {
                clothingType.IsHidden = true; // Đánh dấu ẩn
                db.SaveChanges();
            }
            TempData["messages"] = new NotiMessage("danger", "Xóa danh mục thành công");
            return RedirectToAction("Trash");
        }
    }
}
