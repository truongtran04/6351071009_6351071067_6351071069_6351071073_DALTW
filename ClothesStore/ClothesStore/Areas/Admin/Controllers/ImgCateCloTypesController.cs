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

namespace ClothesStore.Areas.Admin.Controllers
{
    public class ImgCateCloTypesController : Controller
    {
        private static string ApiKey = "AIzaSyC5lutwuFKU0EGGxIq7SrPSlgsy5uKwulE";
        private static string Bucket = "shopping-e2473.appspot.com";
        private static string AuthEmail = "trtr2k4@gmail.com";
        private static string AuthPassword = "Truong@2004";

        private TESTEntities db = new TESTEntities();

        CategoriesDAO categoriesDAO = new CategoriesDAO();

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

        // GET: Admin/ImgCateCloTypes
        public ActionResult Index()
        {
            var category_ClothingType = db.Category_ClothingType.Include(c => c.Category).Include(c => c.ClothingType);
            return View(category_ClothingType.ToList());
        }

        // GET: Admin/ImgCateCloTypes/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category_ClothingType category_ClothingType = db.Category_ClothingType.Find(id);
            if (category_ClothingType == null)
            {
                return HttpNotFound();
            }
            return View(category_ClothingType);
        }

        // GET: Admin/ImgCateCloTypes/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(categoriesDAO.getList("Index"), "CategoryID", "CategoryName");
            ViewBag.ClothingTypeID = new SelectList(db.ClothingTypes.Where(clo => clo.IsHidden == false), "ClothingTypeID", "ClothingTypeName");
            return View();
        }

        // POST: Admin/ImgCateCloTypes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Category_ClothingType category_ClothingType, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                // Upload file
                file = Request.Files["Img"]; // Get file information
                FileStream fileStream;
                if (file.ContentLength != 0)
                {
                    string[] FileExtensions = new string[] { ".jpg", ".jpeg", ".png" };
                    // Validate file extension
                    if (FileExtensions.Contains(Path.GetExtension(file.FileName)))
                    {
                        // Upload image
                        string folderCate = SelectItem.CategoriesID(category_ClothingType.CategoryID);
                        string imageName = XString.Str_Slug(category_ClothingType.CateCloTypeName + " " + folderCate) + Path.GetExtension(file.FileName);
                        category_ClothingType.Img = imageName;
                        string localPath = $"~/Content/images/categories/{folderCate}/";
                        string PathFile = Path.Combine(Server.MapPath(localPath), imageName);

                        // Save image locally
                        file.SaveAs(PathFile);

                        // Create a FileStream to pass to Firebase
                        fileStream = new FileStream(PathFile, FileMode.Open);
                        string firebaseUrl = await Upload(fileStream, $"categories/{folderCate}/{imageName}");

                        // Save Firebase URL or local path to the database
                        category_ClothingType.UrlImg = firebaseUrl;  // Or use the local path if needed

                        db.Category_ClothingType.Add(category_ClothingType);
                        await db.SaveChangesAsync();
                    }
                }
                TempData["messages"] = new NotiMessage("success", "Thêm thành công");
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(categoriesDAO.getList("Index"), "CategoryID", "CategoryName", category_ClothingType.CategoryID);
            ViewBag.ClothingTypeID = new SelectList(db.ClothingTypes.Where(clo => clo.IsHidden == false), "ClothingTypeID", "ClothingTypeName", category_ClothingType.ClothingTypeID);
            return View(category_ClothingType);
        }

        // GET: Admin/ImgCateCloTypes/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category_ClothingType category_ClothingType = db.Category_ClothingType.Find(id);
            if (category_ClothingType == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(categoriesDAO.getList("Index"), "CategoryID", "CategoryName", category_ClothingType.CategoryID);
            ViewBag.ClothingTypeID = new SelectList(db.ClothingTypes, "ClothingTypeID", "ClothingTypeName", category_ClothingType.ClothingTypeID);
            return View(category_ClothingType);
        }

        // POST: Admin/ImgCateCloTypes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,ClothingTypeID,Img,UrlImg")] Category_ClothingType category_ClothingType)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category_ClothingType).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", category_ClothingType.CategoryID);
            ViewBag.ClothingTypeID = new SelectList(db.ClothingTypes, "ClothingTypeID", "ClothingTypeName", category_ClothingType.ClothingTypeID);
            return View(category_ClothingType);
        }

        // GET: Admin/ImgCateCloTypes/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category_ClothingType category_ClothingType = db.Category_ClothingType.Find(id);
            if (category_ClothingType == null)
            {
                return HttpNotFound();
            }
            return View(category_ClothingType);
        }

        // POST: Admin/ImgCateCloTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Category_ClothingType category_ClothingType = db.Category_ClothingType.Find(id);
            db.Category_ClothingType.Remove(category_ClothingType);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
