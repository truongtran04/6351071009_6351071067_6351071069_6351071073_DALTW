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
    public class QuantityController : Controller
    {
        private TESTEntities db = new TESTEntities();

        // GET: Admin/Quantity
        public ActionResult Index()
        {
            var clothes_Color_Size = db.Clothes_Color_Size.Include(c => c.Cloth).Include(c => c.Color).Include(c => c.Size);
            return View(clothes_Color_Size.ToList());
        }

        // GET: Admin/Clothes_Color_Size/Details
        public ActionResult Details(string clothesId, string colorId, string sizeId)
        {
            if (string.IsNullOrEmpty(clothesId) || string.IsNullOrEmpty(colorId) || string.IsNullOrEmpty(sizeId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Clothes_Color_Size clothes_Color_Size = db.Clothes_Color_Size
                .FirstOrDefault(c => c.ClothesID == clothesId && c.ColorID == colorId && c.SizeID == sizeId);

            if (clothes_Color_Size == null)
            {
                return HttpNotFound();
            }

            return View(clothes_Color_Size);
        }
        

        [HttpPost]
        public JsonResult GetColorsByClothesID(string clothesId)
        {
            if (string.IsNullOrEmpty(clothesId))
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            // Truy vấn danh sách màu từ bảng Image dựa trên ClothesID
            var colors = db.Images
                           .Where(i => i.ClothesID == clothesId)
                           .Select(i => new { i.Color.ColorID, i.Color.ColorName })
                           .Distinct()
                           .ToList();

            return Json(colors, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create(string idClothes)
        {
            var cloth = db.Clothes.Find(idClothes);
            if (cloth == null)
            {
                return HttpNotFound("Sản phẩm không tồn tại");
            }

            ViewBag.ClothesName = cloth;
            ViewBag.SizeList = db.Sizes.ToList(); // Truyền SizeList để sử dụng trong View

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ClothesID")] Clothes_Color_Size clothes_Color_Size, string[] selectedColors)
        {
            if (ModelState.IsValid)
            {
                if (selectedColors != null && selectedColors.Any())
                {
                    foreach (var colorId in selectedColors)
                    {
                        // Lấy size và quantity chính cho màu
                        var sizeId = Request.Form[$"size_{colorId}"];
                        var quantity = Convert.ToInt32(Request.Form[$"quantity_{colorId}"]);

                        // Kiểm tra và lưu vào database
                        if (!string.IsNullOrEmpty(colorId) && !string.IsNullOrEmpty(sizeId) && quantity > 0)
                        {
                            var newClothesColorSize = new Clothes_Color_Size
                            {
                                ClothesID = clothes_Color_Size.ClothesID,
                                ColorID = colorId,
                                SizeID = sizeId,
                                Quantity = quantity
                            };

                            db.Clothes_Color_Size.Add(newClothesColorSize);
                        }

                        // Lấy các size và quantity bổ sung
                        var additionalSizes = Request.Form.GetValues($"additionalSize_{colorId}[]");
                        var additionalQuantities = Request.Form.GetValues($"additionalQuantity_{colorId}[]");

                        if (additionalSizes != null && additionalQuantities != null)
                        {
                            for (int i = 0; i < additionalSizes.Length; i++)
                            {
                                var additionalSizeId = additionalSizes[i];
                                var additionalQuantity = Convert.ToInt32(additionalQuantities[i]);

                                if (!string.IsNullOrEmpty(additionalSizeId) && additionalQuantity > 0)
                                {
                                    var additionalClothesColorSize = new Clothes_Color_Size
                                    {
                                        ClothesID = clothes_Color_Size.ClothesID,
                                        ColorID = colorId,
                                        SizeID = additionalSizeId,
                                        Quantity = additionalQuantity
                                    };

                                    db.Clothes_Color_Size.Add(additionalClothesColorSize);
                                }
                            }
                        }
                    }
                    db.SaveChanges();
                    TempData["messages"] = new NotiMessage("success", "Thêm thành công");
                    return RedirectToAction("Details", "Clothes", new { id = clothes_Color_Size.ClothesID });
                }
                else
                {
                    ModelState.AddModelError("", "Bạn phải chọn ít nhất một màu.");
                }
            }

            ViewBag.ClothesList = db.Clothes.ToList();
            ViewBag.SizeList = db.Sizes.ToList();
            return View(clothes_Color_Size);
        }

        // GET: Admin/Quantity/Edit/5
        // GET: Admin/Clothes_Color_Size/Edit
        public ActionResult Edit(string clothesId, string colorId, string sizeId)
        {
            if (string.IsNullOrEmpty(clothesId) || string.IsNullOrEmpty(colorId) || string.IsNullOrEmpty(sizeId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Clothes_Color_Size clothes_Color_Size = db.Clothes_Color_Size
                .FirstOrDefault(c => c.ClothesID == clothesId && c.ColorID == colorId && c.SizeID == sizeId);

            if (clothes_Color_Size == null)
            {
                return HttpNotFound();
            }

            ViewBag.ClothesID = new SelectList(db.Clothes, "ClothesID", "ClothesName", clothes_Color_Size.ClothesID);
            ViewBag.ColorID = new SelectList(db.Colors, "ColorID", "ColorName", clothes_Color_Size.ColorID);
            ViewBag.SizeID = new SelectList(db.Sizes, "SizeID", "SizeName", clothes_Color_Size.SizeID);
            return View(clothes_Color_Size);
        }

        // POST: Admin/Clothes_Color_Size/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ClothesID,ColorID,SizeID,Quantity")] Clothes_Color_Size clothes_Color_Size)
        {
            if (ModelState.IsValid)
            {
                db.Entry(clothes_Color_Size).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ClothesID = new SelectList(db.Clothes, "ClothesID", "ClothesName", clothes_Color_Size.ClothesID);
            ViewBag.ColorID = new SelectList(db.Colors, "ColorID", "ColorName", clothes_Color_Size.ColorID);
            ViewBag.SizeID = new SelectList(db.Sizes, "SizeID", "SizeName", clothes_Color_Size.SizeID);
            return View(clothes_Color_Size);
        }


        // GET: Admin/Clothes_Color_Size/Delete
        public ActionResult Delete(string sizeId, string colorId, string clothesId)
        {
            if (string.IsNullOrEmpty(sizeId) || string.IsNullOrEmpty(colorId) || string.IsNullOrEmpty(clothesId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Tìm bản ghi dựa trên ClothesID, ColorID, và SizeID
            var clothes_Color_Size = db.Clothes_Color_Size
                                        .FirstOrDefault(c => c.SizeID == sizeId && c.ColorID == colorId && c.ClothesID == clothesId);

            if (clothes_Color_Size == null)
            {
                return HttpNotFound();
            }

            return View(clothes_Color_Size);
        }


        // POST: Admin/Clothes_Color_Size/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string sizeId, string colorId, string clothesId)
        {
            if (string.IsNullOrEmpty(sizeId) || string.IsNullOrEmpty(colorId) || string.IsNullOrEmpty(clothesId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Tìm bản ghi dựa trên ClothesID, ColorID, và SizeID
            var clothes_Color_Size = db.Clothes_Color_Size
                                        .FirstOrDefault(c => c.SizeID == sizeId && c.ColorID == colorId && c.ClothesID == clothesId);

            if (clothes_Color_Size == null)
            {
                return HttpNotFound();
            }

            // Xóa bản ghi
            db.Clothes_Color_Size.Remove(clothes_Color_Size);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult GetQuantity(string idClothes)
        {
            var quantity = db.Clothes_Color_Size.Where(q => q.ClothesID == idClothes).ToList();
            ViewBag.ClothesID = idClothes;
            return PartialView("_GetQuantityByClothes", quantity);
        }
    }
}
