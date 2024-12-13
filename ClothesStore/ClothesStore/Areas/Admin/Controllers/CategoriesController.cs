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
using ClothesStore.Models.DAO;

namespace ClothesStore.Areas.Admin.Controllers
{
    public class CategoriesController : Controller
    {
        CategoriesDAO categoriesDAO = new CategoriesDAO();

        // GET: Admin/Categories
        public ActionResult Index()
        {
            return View(categoriesDAO.getList("Index"));
        }
        public ActionResult Trash()
        {
            return View(categoriesDAO.getList("Trash"));
        }
        // GET: Admin/Categories/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = categoriesDAO.getRow(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Admin/Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.CategoryID == null)
                {
                    category.CategoryID = "CATE000";
                }
                categoriesDAO.Insert(category);
                TempData["messages"] = new NotiMessage("success", "Thêm thành công danh mục");
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Admin/Categories/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = categoriesDAO.getRow(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                categoriesDAO.Update(category);
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = categoriesDAO.getRow(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Category category = categoriesDAO.getRow(id);
            if (category != null)
            {
                category.IsHidden = true; // Đánh dấu ẩn
                categoriesDAO.Delete();
            }
            TempData["messages"] = new NotiMessage("danger", "Xóa danh mục thành công");
            return RedirectToAction("Index");
        }
    }
}
