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
using ClothesStore.Models.DAO;
using Firebase.Auth;
using Firebase.Storage;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace ClothesStore.Areas.Admin.Controllers
{
    public class ClothesController : Controller
    {
        private static string ApiKey = "AIzaSyC5lutwuFKU0EGGxIq7SrPSlgsy5uKwulE";
        private static string Bucket = "shopping-e2473.appspot.com";
        private static string AuthEmail = "trtr2k4@gmail.com";
        private static string AuthPassword = "Truong@2004";

        private TESTEntities db = new TESTEntities();

        CategoriesDAO categoriesDAO = new CategoriesDAO();

        // GET: Admin/Clothes
        public ActionResult Index()
        {
            var clothes = db.Clothes.Include(c => c.Category).Include(c => c.ClothingType).Include(c => c.ClothingStyle);
            return View(clothes.Where(clo => clo.IsDeleted == false).OrderByDescending(clo => clo.CreatedAt).ToList());
        }

        public ActionResult Trash()
        {
            var clothes = db.Clothes.Include(c => c.Category).Include(c => c.ClothingType).Include(c => c.ClothingStyle);
            return View(clothes.Where(clo => clo.IsDeleted == true).ToList());
        }

        // GET: Admin/Clothes/Details/5
        public ActionResult Details(string id)
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
            return View(cloth);
        }
        public JsonResult GetClothingStyles(string clothingTypeID)
        {
            var clothingStyles = db.ClothingStyles
                                   .Where(cs => cs.ClothingTypeID == clothingTypeID && cs.IsHidden == false)
                                   .Select(cs => new { cs.ClothingStyleID, cs.ClothingStyleName })
                                   .ToList();
            return Json(clothingStyles, JsonRequestBehavior.AllowGet);
        }
        // GET: Admin/Clothes/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(categoriesDAO.getList("Index"), "CategoryID", "CategoryName");
            ViewBag.ClothingTypeID = new SelectList(db.ClothingTypes.Where(clo => clo.IsHidden == false), "ClothingTypeID", "ClothingTypeName");
            ViewBag.ClothingStyleID = new SelectList(db.ClothingStyles.Where(clo => clo.IsHidden == false), "ClothingStyleID", "ClothingStyleName");
            return View();
        }

        // POST: Admin/Clothes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Cloth cloth, HttpPostedFileBase file)
        {

            if (ModelState.IsValid)
            {
                if (cloth.ClothesID == null)
                {
                    cloth.ClothesID = "CLT000"; // Set default ID
                }

                // Upload file
                file = Request.Files["MainImage"]; // Get file information
                FileStream fileStream;
                if (file.ContentLength != 0)
                {
                    string[] FileExtensions = new string[] { ".jpg", ".jpeg", ".png" };

                    // Validate file extension
                    if (FileExtensions.Contains(Path.GetExtension(file.FileName)))
                    {
                        // Generate image name
                        string folderClothes = XString.Str_Slug(cloth.ClothesName);
                        string imageName = folderClothes + Path.GetExtension(file.FileName);
                        cloth.MainImage = imageName;

                        // Folder paths
                        string folderCategory = SelectItem.CategoriesID(cloth.CategoryID);
                        string folderCloTypes = SelectItem.ClothingTypesID(cloth.ClothingTypeID);
                        string folderCloStyles = SelectItem.ClothingStylesID(cloth.ClothingStyleID);

                        string localPath = $"~/Content/images/clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/";
                        string physicalPath = Server.MapPath(localPath); // Convert virtual path to physical path

                        // Create directories if they do not exist
                        Directory.CreateDirectory(physicalPath); // Automatically creates folder structure

                        string PathFile = Path.Combine(physicalPath, imageName);

                        // Save image locally
                        file.SaveAs(PathFile);

                        // Create a FileStream to pass to Firebase
                        fileStream = new FileStream(PathFile, FileMode.Open);
                        string firebaseUrl = await Upload(fileStream, $"clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/{imageName}");

                        // Save Firebase URL or local path to the database
                        cloth.UrlImage = firebaseUrl;  // Or use the local path if needed
                    }
                }
                // Add category to the database
                db.Clothes.Add(cloth);
                await db.SaveChangesAsync();
            }
            ViewBag.CategoryID = new SelectList(categoriesDAO.getList("Index"), "CategoryID", "CategoryName", cloth.CategoryID);
            ViewBag.ClothingTypeID = new SelectList(db.ClothingTypes.Where(clo => clo.IsHidden == false), "ClothingTypeID", "ClothingTypeName", cloth.ClothingTypeID);
            ViewBag.ClothingStyleID = new SelectList(db.ClothingStyles.Where(clo => clo.IsHidden == false), "ClothingStyleID", "ClothingStyleName", cloth.ClothingStyleID);
            TempData["messages"] = new NotiMessage("success", "Thêm thành công");
            return RedirectToAction("Index");
        }

        // Firebase Upload function
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
        

        // GET: Admin/Clothes/Edit/5
        public ActionResult Edit(string id)
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
            CategoriesDAO categoriesDAO = new CategoriesDAO();
            var clothingStyles = db.ClothingStyles.Where(row => row.ClothingTypeID == cloth.ClothingTypeID && row.IsHidden == false);
            ViewBag.CategoryID = new SelectList(categoriesDAO.getList("Index"), "CategoryID", "CategoryName", cloth.CategoryID);
            ViewBag.ClothingTypeID = new SelectList(db.ClothingTypes.Where(clo => clo.IsHidden == false), "ClothingTypeID", "ClothingTypeName", cloth.ClothingTypeID);
            ViewBag.ClothingStyleID = new SelectList(clothingStyles, "ClothingStyleID", "ClothingStyleName", cloth.ClothingStyleID);
            return View(cloth);
        }

        // POST: Admin/Clothes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Cloth cloth, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                // Upload file
                file = Request.Files["MainImage"]; // Get file information
                FileStream fileStream;
                if (file.ContentLength != 0)
                {
                    string[] FileExtensions = new string[] { ".jpg", ".jpeg", ".png" };

                    // Validate file extension
                    if (FileExtensions.Contains(Path.GetExtension(file.FileName)))
                    {
                        // Generate image name
                        string folderClothes = XString.Str_Slug(cloth.ClothesName);
                        string imageName = folderClothes + Path.GetExtension(file.FileName);
                        cloth.MainImage = imageName;

                        // Folder paths
                        string folderCategory = SelectItem.CategoriesID(cloth.CategoryID);
                        string folderCloTypes = SelectItem.ClothingTypesID(cloth.ClothingTypeID);
                        string folderCloStyles = SelectItem.ClothingStylesID(cloth.ClothingStyleID);

                        string localPath = $"~/Content/images/clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/";
                        string physicalPath = Server.MapPath(localPath); // Convert virtual path to physical path
                        string PathFile = Path.Combine(physicalPath, imageName);

                        string LinkImg = $"clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/{cloth.MainImage}";

                        if (cloth.MainImage != null)
                        {
                            string DelFile = Path.Combine(Server.MapPath(localPath), cloth.MainImage);
                            System.IO.File.Delete(DelFile); // xóa hình cũ

                            // Delete the image from Firebase Storage
                            await DeleteImageFromFirebase(LinkImg);

                        }

                        // Save image locally
                        file.SaveAs(PathFile);

                        // Create a FileStream to pass to Firebase
                        fileStream = new FileStream(PathFile, FileMode.Open);
                        string firebaseUrl = await Upload(fileStream, $"clothes/{folderCategory}/{folderCloTypes}/{folderCloStyles}/{folderClothes}/{imageName}");

                        // Save Firebase URL or local path to the database
                        cloth.UrlImage = firebaseUrl;  // Or use the local path if needed
                    }
                }
                // Fetch the original cloth record to retain the CreatedAt value
                var existingCloth = db.Clothes.AsNoTracking().FirstOrDefault(c => c.ClothesID == cloth.ClothesID);
                if (existingCloth != null)
                {
                    // Retain the original CreatedAt value
                    cloth.CreatedAt = existingCloth.CreatedAt;

                    // Update the UpdatedAt value to current time
                    cloth.UpdatedAt = DateTime.Now;

                    db.Entry(cloth).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

            CategoriesDAO categoriesDAO = new CategoriesDAO();
            var clothingStyles = db.ClothingStyles.Where(row => row.ClothingTypeID == cloth.ClothingTypeID && row.IsHidden == false);
            ViewBag.CategoryID = new SelectList(categoriesDAO.getList("Index"), "CategoryID", "CategoryName", cloth.CategoryID);
            ViewBag.ClothingTypeID = new SelectList(db.ClothingTypes.Where(clo => clo.IsHidden == false), "ClothingTypeID", "ClothingTypeName", cloth.ClothingTypeID);
            ViewBag.ClothingStyleID = new SelectList(clothingStyles, "ClothingStyleID", "ClothingStyleName", cloth.ClothingStyleID);

            return View(cloth);
        }


        // GET: Admin/Clothes/Delete/5
        public ActionResult Delete(string id)
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
            return View(cloth);
        }

        // POST: Admin/Clothes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Cloth cloth = db.Clothes.Find(id);
            cloth.IsDeleted = true;
            db.SaveChanges();
            TempData["messages"] = new NotiMessage("danger", "Xóa danh mục thành công");
            return RedirectToAction("Trash");
        }
        // Method to delete from Firebase
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
    }
}
