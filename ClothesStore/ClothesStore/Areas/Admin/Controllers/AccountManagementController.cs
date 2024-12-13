using ClothesStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ClothesStore.Areas.Admin.Controllers
{
    public class AccountManagementController : Controller
    {
        private TESTEntities db = new TESTEntities();
        // GET: Admin/AccountManagement
        public ActionResult Index()
        {
            var users = db.Users.ToList()
                .Where(u => !db.UserRoles.Any(ur => ur.UserId == u.Id && ur.Role == "admin"))
                .ToList();
            return View(users);
        }

        [HttpGet]
        public ActionResult Deactivate(int id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                user.IsActive = false; // Đặt trạng thái là không hoạt động
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Activate(int id)
        {
            var user = db.Users.Find(id);
            if (user != null)
            {
                user.IsActive = true; // Đặt trạng thái là hoạt động
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}