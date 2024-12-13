using ClothesStore.Models;
using ClothesStore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace ClothesStore.Controllers
{
    public class ClothesController : Controller
    {
        private TESTEntities db = new TESTEntities();
        // GET: Clothes
        public ActionResult Index(string typeName)
        {
            ViewBag.Category = db.Categories.Where(cate => cate.IsHidden == false).ToList();

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
            var clothes = clothesViewModel.Where(clo => clo.ClothesItem.ClothingType.ClothingTypeName == typeName).ToList();
            return View(clothes);
        }
        public ActionResult GetClothesCate(string idCate, string idType)
        {
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

            var clothes = clothesViewModel.Where(clo => clo.ClothesItem.CategoryID == idCate && clo.ClothesItem.ClothingTypeID == idType).ToList();

            return PartialView("_GetClothesCol_3", clothes);
        }

        public ActionResult GetClothesByCateAll(string idType){
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

            var clothes = clothesViewModel.Where(clo => clo.ClothesItem.ClothingTypeID == idType).ToList();

            return PartialView("_GetClothesCol_3", clothes);
        }
        public ActionResult GetClothesByCate(string categoryId)
        {
            CategoryModels clothesModel = new CategoryModels();
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

            clothesModel.clothes = clothesViewModel.Where(clo => clo.ClothesItem.CategoryID == categoryId).ToList();

            clothesModel.clothingTypes = db.Category_ClothingType.Where(cate => cate.CategoryID == categoryId).ToList();


            var category = db.Categories.Find(categoryId);
            if (category.CategoryName == "Nam")
            {
                ViewBag.AoKhoac = Url.Content("~/Content/images/banner/clothingType/nam/Ao_khoac_nam.png");
                ViewBag.AoNi = Url.Content("~/Content/images/banner/clothingType/nam/Ao_ni_nam.png");
                ViewBag.AoLen = Url.Content("~/Content/images/banner/clothingType/nam/Ao_len_nam.png");
                ViewBag.AoPhong = Url.Content("~/Content/images/banner/clothingType/nam/Ao_phong_nam.png");
            }
            else if (category.CategoryName == "Nữ")
            {
                ViewBag.AoKhoac = Url.Content("~/Content/images/banner/clothingType/nu/ao_khoac_nu.png");
                ViewBag.AoNi = Url.Content("~/Content/images/banner/clothingType/nu/ao_ni_nu.png");
                ViewBag.AoLen = Url.Content("~/Content/images/banner/clothingType/nu/ao_len_nu.png");
                ViewBag.AoPhong = Url.Content("~/Content/images/banner/clothingType/nu/vay_nu.png");
            }
            else if (category.CategoryName == "Bé gái")
            {
                ViewBag.AoKhoac = Url.Content("~/Content/images/banner/clothingType/be-gai/ao_khoac_be_gai.png");
                ViewBag.AoNi = Url.Content("~/Content/images/banner/clothingType/be-gai/ao_ni_be_gai.png");
                ViewBag.AoLen = Url.Content("~/Content/images/banner/clothingType/be-gai/vay_be_gai.png");
                ViewBag.AoPhong = Url.Content("~/Content/images/banner/clothingType/be-gai/mac_nha_be_gai.png");
            }
            else if (category.CategoryName == "Bé trai")
            {
                ViewBag.AoKhoac = Url.Content("~/Content/images/banner/clothingType/be-trai/ao_khoac_be_trai.png");
                ViewBag.AoNi = Url.Content("~/Content/images/banner/clothingType/be-trai/ao_ni_be_trai.png");
                ViewBag.AoLen = Url.Content("~/Content/images/banner/clothingType/be-trai/bo_quan_ao_be_trai.png");
                ViewBag.AoPhong = Url.Content("~/Content/images/banner/clothingType/be-trai/mac_nha_be_trai.png");
            }
            else
            {
                ViewBag.AoKhoac = Url.Content("https://placehold.co/600x800");
                ViewBag.AoNi = Url.Content("https://placehold.co/600x800");
                ViewBag.AoLen = Url.Content("https://placehold.co/600x800");
                ViewBag.AoPhong = Url.Content("https://placehold.co/600x800");
                ViewBag.Image = Url.Content("https://placehold.co/600x800");
            }

            return View(clothesModel);
        }

        public ActionResult _ClothingType()
        {
            var cloType = db.Category_ClothingType.Where(clo => clo.IsHidden ==  false).ToList(); 
            return View(cloType);
        }

        public ActionResult _ClothingStyle()
        {
            var cloStyle = db.ClothingStyles.Where(clo => clo.IsHidden == false).ToList();
            return View(cloStyle);
        }

        public ActionResult GetClothesByType(string idCate, string idType)
        {
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
            var clothes = clothesViewModel.Where(clo => clo.ClothesItem.CategoryID == idCate && clo.ClothesItem.ClothingTypeID == idType).ToList();
            ViewBag.Count = clothes.Count;

            var cloType = db.Category_ClothingType.Where(clo => clo.IsHidden == false && clo.CategoryID == idCate).ToList();
            ViewBag.ClothingType = cloType;

            var cloStyle = db.ClothingStyles.Where(clo => clo.ClothingTypeID == idType && clo.IsHidden == false).ToList();
            ViewBag.ClothesStyle = cloStyle;

            return View(clothes);
        }
        public ActionResult GetClothesByTypeAll(string idCate, string idType)
        {
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

            var clothes = clothesViewModel.Where(clo => clo.ClothesItem.CategoryID == idCate && clo.ClothesItem.ClothingTypeID == idType).ToList();

            return PartialView("_GetClothesCol_4", clothes);
        }

        public ActionResult GetClothesByStyle(string idCate, string idStyle)
        {
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

            var clothes = clothesViewModel.Where(clo => clo.ClothesItem.CategoryID == idCate && clo.ClothesItem.ClothingStyleID == idStyle).ToList();

            return PartialView("_GetClothesCol_4", clothes);
        }
    }
}