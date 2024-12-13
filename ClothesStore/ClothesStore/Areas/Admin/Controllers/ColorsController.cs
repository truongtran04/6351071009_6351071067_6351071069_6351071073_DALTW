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
using System.Runtime.InteropServices.ComTypes;

namespace ClothesStore.Areas.Admin.Controllers
{
    public class ColorsController : Controller
    {
        private static string ApiKey = "AIzaSyC5lutwuFKU0EGGxIq7SrPSlgsy5uKwulE";
        private static string Bucket = "shopping-e2473.appspot.com";
        private static string AuthEmail = "trtr2k4@gmail.com";
        private static string AuthPassword = "Truong@2004";

        private TESTEntities db = new TESTEntities();

        // GET: Admin/Colors
        public ActionResult Index()
        {
            return View(db.Colors.ToList());
        }

        // GET: Admin/Colors/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Color color = db.Colors.Find(id);
            if (color == null)
            {
                return HttpNotFound();
            }
            return View(color);
        }

        // GET: Admin/Colors/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Colors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Color color, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                if (color.ColorID == null)
                {
                    color.ColorID = "C000";
                }
                // Upload file
                file = Request.Files["ImageColor"]; // Get file information
                FileStream fileStream;
                if (file.ContentLength != 0)
                {
                    string[] FileExtensions = new string[] { ".jpg", ".jpeg", ".png" };
                    // Validate file extension
                    if (FileExtensions.Contains(Path.GetExtension(file.FileName)))
                    {
                        // Upload image
                        string imageName = XString.Str_Slug(color.ColorName) + Path.GetExtension(file.FileName);
                        color.ImageColor = imageName;
                        string localPath = "~/Content/images/colors";
                        string PathFile = Path.Combine(Server.MapPath(localPath), imageName);

                        // Save image locally
                        file.SaveAs(PathFile);

                        // Create a FileStream to pass to Firebase
                        fileStream = new FileStream(PathFile, FileMode.Open);
                        string firebaseUrl = await Upload(fileStream, $"colors/{imageName}");

                        // Save Firebase URL or local path to the database
                        color.UrlImage = firebaseUrl;  // Or use the local path if needed

                        // Add category to the database
                        db.Colors.Add(color);
                        await db.SaveChangesAsync();
                    }
                }
            }
            TempData["messages"] = new NotiMessage("success", "Thêm thành công");
            return RedirectToAction("Index");
        }
        // GET: Admin/Colors/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Color color = db.Colors.Find(id);
            if (color == null)
            {
                return HttpNotFound();
            }
            return View(color);
        }

        // POST: Admin/Colors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Color color, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                // Upload file
                file = Request.Files["ImageColor"]; // Get file information
                FileStream fileStream;
                if (file.ContentLength != 0)
                {
                    string[] FileExtensions = new string[] { ".jpg", ".jpeg", ".png" };
                    // Validate file extension
                    if (FileExtensions.Contains(Path.GetExtension(file.FileName)))
                    {
                        // Upload image
                        string imageName = XString.Str_Slug(color.ColorName) + Path.GetExtension(file.FileName);
                        color.ImageColor = imageName;
                        string localPath = "~/Content/images/colors";
                        string PathFile = Path.Combine(Server.MapPath(localPath), imageName);

                        string link = $"colors/{color.ImageColor}";

                        if (color.ImageColor.Length > 0)
                        {
                            string DelFile = Path.Combine(Server.MapPath(localPath), color.ImageColor);
                            System.IO.File.Delete(DelFile); // xóa hình cũ
                            // Delete the image from Firebase Storage
                            await DeleteImageFromFirebase(link);
                        }

                        // Save image locally
                        file.SaveAs(PathFile);

                        // Create a FileStream to pass to Firebase
                        fileStream = new FileStream(PathFile, FileMode.Open);
                        string firebaseUrl = await Upload(fileStream, $"colors/{imageName}");

                        // Save Firebase URL or local path to the database
                        color.UrlImage = firebaseUrl;  // Or use the local path if needed

                    }
                }
                db.Entry(color).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(color);
        }

        // GET: Admin/Colors/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Color color = db.Colors.Find(id);
            if (color == null)
            {
                return HttpNotFound();
            }
            return View(color);
        }

        // POST: Admin/Colors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Color color = db.Colors.Find(id);

            string localPath = "~/Content/images/colors";
            string link = $"colors/{color.ImageColor}";

            if (color.ImageColor.Length > 0)
            {
                string DelFile = Path.Combine(Server.MapPath(localPath), color.ImageColor);
                System.IO.File.Delete(DelFile); // xóa hình cũ
                                                // Delete the image from Firebase Storage
                await DeleteImageFromFirebase(link);
            }

            db.Colors.Remove(color);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
