using ClothesStore.Models;
using ClothesStore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ClothesStore.Controllers
{
    public class ClothesDetailController : Controller
    {
        // GET: ClothesDetail
        private TESTEntities db = new TESTEntities();
        // GET: ClothesDetail
        public ActionResult Index(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cloth cloth = db.Clothes.Find(id);
            if (cloth == null)
            {
                return HttpNotFound();
            }

            ViewBag.Categories = db.Categories.ToList();
            ViewBag.ClothingTypes = db.ClothingTypes.ToList();
            ViewBag.Clothes = db.Clothes.ToList();
            ViewBag.Images = db.Images.ToList();
            ViewBag.Colors = db.Colors.ToList();
            ViewBag.Sizes = db.Sizes.ToList();
            ViewBag.Quantity = db.Clothes_Color_Size.ToList();

            var clothesViewModel = db.Clothes.Where(clo => clo.IsDeleted == false)
             .Select(c => new ClothesViewModel
             {
                 ClothesItem = c,
                 Images = db.Images.Where(img => img.ClothesID == c.ClothesID).ToList(),
                 Clothes_Color_Sizes = db.Clothes_Color_Size.Where(clo => clo.ClothesID == c.ClothesID).ToList(),

                 Colors = db.Colors
                     .Where(col => db.Images.Any(img => img.ColorID == col.ColorID && img.ClothesID == c.ClothesID))
                     .ToList(),

                 // Get sizes associated with each color for the clothing item
                 Sizes = db.Sizes
                     .Where(size => db.Clothes_Color_Size
                         .Any(clo => clo.SizeID == size.SizeID && clo.ClothesID == c.ClothesID
                                      && clo.ColorID == db.Colors
                                         .Where(col => db.Images.Any(img => img.ColorID == col.ColorID && img.ClothesID == c.ClothesID))
                                         .Select(col => col.ColorID).FirstOrDefault()))
                     .ToList()
             })
             .ToList();

            var clothes = clothesViewModel.Where(clo => clo.ClothesItem.CategoryID == cloth.CategoryID && clo.ClothesItem.ClothingTypeID != cloth.CategoryID).ToList();

            ViewBag.Clothes = clothes;

            return View(cloth);
        }
    }
}