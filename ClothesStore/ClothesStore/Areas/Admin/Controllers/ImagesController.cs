using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using ClothesStore.Library;
using ClothesStore.Models;
using Firebase.Auth;
using Firebase.Storage;

namespace ClothesStore.Areas.Admin.Controllers
{
    public class ImagesController : Controller
    {
        private static string ApiKey = "AIzaSyC5lutwuFKU0EGGxIq7SrPSlgsy5uKwulE";
        private static string Bucket = "shopping-e2473.appspot.com";
        private static string AuthEmail = "trtr2k4@gmail.com";
        private static string AuthPassword = "Truong@2004";

        private TESTEntities db = new TESTEntities();

        // GET: Admin/Images
        public ActionResult Index()
        {
            var images = db.Images.Include(i => i.Cloth).Include(i => i.Color);
            return View(images.ToList());
        }

        // GET: Admin/Images/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Image image = db.Images.Find(id);
            if (image == null)
            {
                return HttpNotFound();
            }
            return View(image);
        }

        public async Task<string> Upload(FileStream fileStream, string fileName)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

            var cancellation = new CancellationTokenSource();

            var task = new FirebaseStorage(
                Bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                    ThrowOnCancel = true
                })
                .Child("images")
                .Child(fileName)
                .PutAsync(fileStream, cancellation.Token);

            try
            {
                // Get the download URL from Firebase
                string link = await task;
                return link;  // Return the Firebase URL
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception was thrown: {0}", ex);
                return null;  // Handle failure case appropriately
            }
        }
        private async Task DeleteImageFromFirebase(string imageUrl)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

            // Create a reference to the file in Firebase Storage
            var cancellation = new CancellationTokenSource();

            var storage = new FirebaseStorage(
                Bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                    ThrowOnCancel = true
                });

            try
            {
                // Call DeleteAsync without arguments
                await storage.Child($"images/{imageUrl}").DeleteAsync();
                Console.WriteLine($"Deleted image: {imageUrl}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while deleting image: {ex.Message}");
            }
        }

        public JsonResult GetColors()
        {
            var colors = db.Colors.Select(c => new
            {
                c.ColorID,
                c.ColorName,
                c.UrlImage // Đường dẫn đến hình ảnh
            }).ToList();
            return Json(colors, JsonRequestBehavior.AllowGet);
        }

        // GET: Admin/Images/Create
        public ActionResult Create(string idClothes)
        {
            var cloth = db.Clothes.Find(idClothes);
            if (cloth == null)
            {
                return HttpNotFound("Sản phẩm không tồn tại");
            }

            ViewBag.ClothesName = cloth;
            return View();
        }

        // POST: Admin/Images/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Image image, HttpPostedFileBase MainImage, HttpPostedFileBase SecondaryImage1, HttpPostedFileBase SecondaryImage2, HttpPostedFileBase SecondaryImage3)
        {
            if (ModelState.IsValid)
            {
                // Assign a default ImageID if not provided
                if (image.ImageID == null)
                {
                    image.ImageID = "IMG000";
                }

                // Retrieve related data using ClothesID
                var cloth = db.Clothes.Find(image.ClothesID); // Assuming you have a database context 'db'
                if (cloth == null)
                {
                    ModelState.AddModelError("ClothesID", "Sản phẩm không hợp lệ.");
                    return View(image);
                }

                // Prepare folder structure
                string folderCategory = SelectItem.CategoriesID(cloth.CategoryID);
                string folderCloTypes = SelectItem.ClothingTypesID(cloth.ClothingTypeID);
                string folderCloStyles = SelectItem.ClothingStylesID(cloth.ClothingStyleID);
                string folderClothes = XString.Str_Slug(cloth.ClothesName);

                var color = db.Colors.Find(image.ColorID); // Assuming you have a database context 'db'
                if (color == null)
                {
                    // Handle the case where the ColorID is invalid
                    ModelState.AddModelError("ColorID", "Invalid ColorID.");
                    return View(image);
                }

                string folderColor = XString.Str_Slug(color.ColorName);
                string localPath = $"~/Content/images/clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/{folderColor}/";
                string physicalPath = Server.MapPath(localPath);
                Directory.CreateDirectory(physicalPath);

                // Supported image extensions
                string[] FileExtensions = new string[] { ".jpg", ".jpeg", ".png" };

                // Process Main Image
                if (MainImage != null && MainImage.ContentLength > 0)
                {
                    if (FileExtensions.Contains(Path.GetExtension(MainImage.FileName)))
                    {
                        string nameMainImg = XString.Str_Slug(image.ImageName + " chinh") + Path.GetExtension(MainImage.FileName);
                        image.MainImage = nameMainImg;
                        string PathFile = Path.Combine(physicalPath, nameMainImg);
                        MainImage.SaveAs(PathFile);

                        string LinkImg = $"clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/{folderColor}/{image.MainImage}";
                        using (FileStream fileMainImg = new FileStream(PathFile, FileMode.Open))
                        {
                            string firebaseUrl = await Upload(fileMainImg, LinkImg);
                            image.UrlMainImg = firebaseUrl;
                        }
                    }
                }

                // Process Secondary Images
                List<HttpPostedFileBase> secondaryImages = new List<HttpPostedFileBase> { SecondaryImage1, SecondaryImage2, SecondaryImage3 };
                for (int i = 0; i < secondaryImages.Count; i++)
                {
                    var secondaryImage = secondaryImages[i];
                    if (secondaryImage != null && secondaryImage.ContentLength > 0)
                    {
                        if (FileExtensions.Contains(Path.GetExtension(secondaryImage.FileName)))
                        {
                            string nameSecImg = XString.Str_Slug(image.ImageName + $" phu-{i + 1}") + Path.GetExtension(secondaryImage.FileName);
                            switch (i)
                            {
                                case 0:
                                    image.SecondaryImage1 = nameSecImg;
                                    break;
                                case 1:
                                    image.SecondaryImage2 = nameSecImg;
                                    break;
                                case 2:
                                    image.SecondaryImage3 = nameSecImg;
                                    break;
                            }

                            string PathFile = Path.Combine(physicalPath, nameSecImg);
                            secondaryImage.SaveAs(PathFile);

                            string LinkImg = $"clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/{folderColor}/{nameSecImg}";
                            using (FileStream fileSecImg = new FileStream(PathFile, FileMode.Open))
                            {
                                string firebaseUrl = await Upload(fileSecImg, LinkImg);
                                switch (i)
                                {
                                    case 0:
                                        image.UrlSecondaryImage1 = firebaseUrl;
                                        break;
                                    case 1:
                                        image.UrlSecondaryImage2 = firebaseUrl;
                                        break;
                                    case 2:
                                        image.UrlSecondaryImage3 = firebaseUrl;
                                        break;
                                }
                            }
                        }
                    }
                }

                // Save the image entity to the database
                db.Images.Add(image);
                await db.SaveChangesAsync();

                TempData["messages"] = new NotiMessage("success", "Thêm thành công");
                // Redirect to Index
                return RedirectToAction("Details", "Clothes", new { id = image.ClothesID });

            }

            ViewBag.ClothesName = db.Clothes.Find(image.ClothesID).ClothesName;
            return View(image);
        }



        // GET: Admin/Images/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Image image = db.Images.Find(id);
            if (image == null)
            {
                return HttpNotFound();
            }
            ViewBag.ClothesID = new SelectList(db.Clothes, "ClothesID", "ClothesName", image.ClothesID);
            ViewBag.ColorID = new SelectList(db.Colors, "ColorID", "ColorName", image.ColorID);
            return View(image);
        }

        // POST: Admin/Images/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Image image, HttpPostedFileBase MainImage, HttpPostedFileBase SecondaryImage1, HttpPostedFileBase SecondaryImage2, HttpPostedFileBase SecondaryImage3)
        {
            if (ModelState.IsValid)
            {
                // Retrieve related data using ClothesID
                var cloth = db.Clothes.Find(image.ClothesID); // Assuming you have a database context 'db'
                if (cloth == null)
                {
                    // Handle the case where the ClothesID is invalid
                    ModelState.AddModelError("ClothesID", "Invalid ClothesID.");
                    return View(image);
                }

                // Prepare folder structure
                string folderCategory = SelectItem.CategoriesID(cloth.CategoryID);
                string folderCloTypes = SelectItem.ClothingTypesID(cloth.ClothingTypeID);
                string folderCloStyles = SelectItem.ClothingStylesID(cloth.ClothingStyleID);
                string folderClothes = XString.Str_Slug(cloth.ClothesName);

                var color = db.Colors.Find(image.ColorID); // Assuming you have a database context 'db'
                if (color == null)
                {
                    // Handle the case where the ColorID is invalid
                    ModelState.AddModelError("ColorID", "Invalid ColorID.");
                    return View(image);
                }

                string folderColor = XString.Str_Slug(color.ColorName);

                string localPath = $"~/Content/images/clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/{folderColor}/";
                string physicalPath = Server.MapPath(localPath);
                Directory.CreateDirectory(physicalPath);

                // Supported image extensions
                string[] FileExtensions = new string[] { ".jpg", ".jpeg", ".png" };

                // Process Main Image
                if (MainImage != null && MainImage.ContentLength > 0)
                {
                    if (FileExtensions.Contains(Path.GetExtension(MainImage.FileName)))
                    {
                        string nameMainImg = XString.Str_Slug(image.ImageName + " chinh") + Path.GetExtension(MainImage.FileName);
                        image.MainImage = nameMainImg;
                        string PathFile = Path.Combine(physicalPath, nameMainImg);

                        string LinkImg = $"clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/{folderColor}/{image.MainImage}";

                        if (image.MainImage != null)
                        {
                            string DelFile = Path.Combine(Server.MapPath(localPath), image.MainImage);
                            System.IO.File.Delete(DelFile); // xóa hình cũ

                            // Delete the image from Firebase Storage
                            await DeleteImageFromFirebase(LinkImg);

                        }

                        MainImage.SaveAs(PathFile);

                        using (FileStream fileMainImg = new FileStream(PathFile, FileMode.Open))
                        {
                            string firebaseUrl = await Upload(fileMainImg, LinkImg);
                            image.UrlMainImg = firebaseUrl;
                        }
                    }
                }

                // Process Secondary Images
                List<HttpPostedFileBase> secondaryImages = new List<HttpPostedFileBase> { SecondaryImage1, SecondaryImage2, SecondaryImage3 };
                for (int i = 0; i < secondaryImages.Count; i++)
                {
                    var secondaryImage = secondaryImages[i];
                    if (secondaryImage != null && secondaryImage.ContentLength > 0)
                    {
                        if (FileExtensions.Contains(Path.GetExtension(secondaryImage.FileName)))
                        {
                            string nameSecImg = XString.Str_Slug(image.ImageName + $" phu-{i + 1}") + Path.GetExtension(secondaryImage.FileName);
                            switch (i)
                            {
                                case 0:
                                    image.SecondaryImage1 = nameSecImg;
                                    break;
                                case 1:
                                    image.SecondaryImage2 = nameSecImg;
                                    break;
                                case 2:
                                    image.SecondaryImage3 = nameSecImg;
                                    break;
                            }

                            string PathFile = Path.Combine(physicalPath, nameSecImg);

                            string LinkImg = $"clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/{folderColor}/{nameSecImg}";

                            if (secondaryImage != null)
                            {
                                string DelFile = Path.Combine(Server.MapPath(localPath), nameSecImg);
                                System.IO.File.Delete(DelFile); // xóa hình cũ

                                // Delete the image from Firebase Storage
                                await DeleteImageFromFirebase(LinkImg);

                            }

                            secondaryImage.SaveAs(PathFile);


                            using (FileStream fileSecImg = new FileStream(PathFile, FileMode.Open))
                            {
                                string firebaseUrl = await Upload(fileSecImg, LinkImg);
                                switch (i)
                                {
                                    case 0:
                                        image.UrlSecondaryImage1 = firebaseUrl;
                                        break;
                                    case 1:
                                        image.UrlSecondaryImage2 = firebaseUrl;
                                        break;
                                    case 2:
                                        image.UrlSecondaryImage3 = firebaseUrl;
                                        break;
                                }
                            }
                        }
                    }
                }
                db.Entry(image).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ClothesID = new SelectList(db.Clothes, "ClothesID", "ClothesName", image.ClothesID);
            ViewBag.ColorID = new SelectList(db.Colors, "ColorID", "ColorName", image.ColorID);
            return View(image);
        }

        // GET: Admin/Images/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Image image = db.Images.Find(id);
            if (image == null)
            {
                return HttpNotFound();
            }
            return View(image);
        }

        // POST: Admin/Images/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Image image = db.Images.Find(id);
            // Retrieve related data using ClothesID
            var cloth = db.Clothes.Find(image.ClothesID); // Assuming you have a database context 'db'
            if (cloth == null)
            {
                // Handle the case where the ClothesID is invalid
                ModelState.AddModelError("ClothesID", "Invalid ClothesID.");
                return View(image);
            }

            // Prepare folder structure
            string folderCategory = SelectItem.CategoriesID(cloth.CategoryID);
            string folderCloTypes = SelectItem.ClothingTypesID(cloth.ClothingTypeID);
            string folderCloStyles = SelectItem.ClothingStylesID(cloth.ClothingStyleID);
            string folderClothes = XString.Str_Slug(cloth.ClothesName);

            var color = db.Colors.Find(image.ColorID); // Assuming you have a database context 'db'
            if (color == null)
            {
                // Handle the case where the ColorID is invalid
                ModelState.AddModelError("ColorID", "Invalid ColorID.");
                return View(image);
            }

            string folderColor = XString.Str_Slug(color.ColorName);

            string localPath = $"~/Content/images/clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/{folderColor}/";
            string LinkMainImg = $"clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/{folderColor}/{image.MainImage}";
            string LinkSeImg1 = $"clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/{folderColor}/{image.SecondaryImage1}";
            string LinkSeImg2 = $"clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/{folderColor}/{image.SecondaryImage2}";
            string LinkSeImg3 = $"clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/{folderColor}/{image.SecondaryImage3}";

            if (image.MainImage != null)
            {
                string DelFile = Path.Combine(Server.MapPath(localPath), image.MainImage);
                System.IO.File.Delete(DelFile); // xóa hình cũ

                // Delete the image from Firebase Storage
                await DeleteImageFromFirebase(LinkMainImg);

            }
            if (image.SecondaryImage1 != null)
            {
                string DelFile = Path.Combine(Server.MapPath(localPath), image.SecondaryImage1);
                System.IO.File.Delete(DelFile); // xóa hình cũ

                // Delete the image from Firebase Storage
                await DeleteImageFromFirebase(LinkSeImg1);

            }
            if (image.MainImage != null)
            {
                string DelFile = Path.Combine(Server.MapPath(localPath), image.SecondaryImage2);
                System.IO.File.Delete(DelFile); // xóa hình cũ

                // Delete the image from Firebase Storage
                await DeleteImageFromFirebase(LinkSeImg2);

            }
            if (image.MainImage != null)
            {
                string DelFile = Path.Combine(Server.MapPath(localPath), image.SecondaryImage3);
                System.IO.File.Delete(DelFile); // xóa hình cũ

                // Delete the image from Firebase Storage
                await DeleteImageFromFirebase(LinkSeImg3);
            }

            db.Images.Remove(image);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult GetImages(string idClothes)
        {
            var images = db.Images.Where(img => img.ClothesID == idClothes).ToList();
            ViewBag.ClothesID = idClothes;
            return PartialView("_GetImagesByClothes", images);
        }
    }
}
