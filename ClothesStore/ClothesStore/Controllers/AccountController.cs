using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClothesStore.Models;
using System.Data.Entity;
using SendGrid;
using SendGrid.Helpers.Mail;
using UserModel = ClothesStore.Models.User; // Đặt alias cho User trong LastDemo.Models
using Firebase.Auth;
using System.Configuration;
using System.Web.Security;
using EllipticCurve.Utils;
using System.Web;
using System.Net.Mail;

public class EmailService
{
    private readonly string _sendGridApiKey;

    public EmailService()
    {
        _sendGridApiKey = ConfigurationManager.AppSettings["SendGridApiKey"];
    }

    public async Task SendOtpEmailAsync(string recipientEmail, string otp)
    {
        var client = new SendGridClient(_sendGridApiKey);
        var from = new EmailAddress("binhtien032@gmail.com", "Canifa");
        var to = new EmailAddress(recipientEmail);
        var subject = "Mã OTP";
        var plainTextContent = $"Mã OTP của bạn là: {otp}";
        var htmlContent = $"<p>Mã OTP của bạn là: <strong>{otp}</strong></p>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        var response = await client.SendEmailAsync(msg);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Body.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"Failed to send email to {recipientEmail}: {responseBody}");
            throw new Exception($"Failed to send email: {responseBody}");
        }
    }

    public async Task SendPasswordResetEmailAsync(string recipientEmail, string resetLink)
    {
        var client = new SendGridClient(_sendGridApiKey);
        var from = new EmailAddress("binhtien032@gmail.com", "Canifa");
        var to = new EmailAddress(recipientEmail);
        var subject = "Khôi phục mật khẩu";
        var plainTextContent = $"Nhấp vào liên kết sau để khôi phục mật khẩu: {resetLink}";
        var htmlContent = $"<p>Nhấp vào liên kết sau để khôi phục mật khẩu: <a href='{resetLink}'>Khôi phục mật khẩu</a></p>";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        await client.SendEmailAsync(msg);
    }
}

public class AccountController : Controller
{
    private TESTEntities db = new TESTEntities(); // Sử dụng DbContext đã có
    private EmailService _emailService = new EmailService(); // Sử dụng service email

    // Action GET Đăng ký
    [HttpGet]
    public ActionResult Register()
    {
        return View();
    }

    // Action Đăng ký
    [HttpPost]
    public ActionResult Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Kiểm tra username có tồn tại không
            var existingUser = db.Users.FirstOrDefault(u => u.Username == model.Username);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Username đã tồn tại.");
                return View(model);
            }

            // Hash mật khẩu
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // Tạo người dùng mới
            var newUser = new UserModel
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            // Lưu vào CSDL
            db.Users.Add(newUser);
            db.SaveChanges();

            // Kiểm tra có tài khoản admin chưa
            var adminExists = db.UserRoles.Any(r => r.Role == "admin");

            // Gán quyền admin nếu chưa có admin, ngược lại gán quyền client
            var userRole = new UserRole
            {
                UserId = newUser.Id,
                Role = adminExists ? "client" : "admin"
            };

            db.UserRoles.Add(userRole);
            db.SaveChanges();

            return RedirectToAction("Login");
        }

        return View(model);
    }

    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Tìm người dùng theo username và kiểm tra trạng thái hoạt động
            var user = db.Users.FirstOrDefault(u => u.Username == model.Username && (u.IsActive ?? false));

            // Kiểm tra nếu người dùng tồn tại
            if (user != null)
            {
                // Kiểm tra mật khẩu
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
                if (!isPasswordValid)
                {
                    ModelState.AddModelError("", "Tên tài khoản hoặc mật khẩu không đúng.");
                    return View(model);
                }

                // Nếu mật khẩu đúng, thiết lập cookie xác thực
                bool rememberMe = model.RememberMe;
                FormsAuthentication.SetAuthCookie(user.Username, rememberMe);

                HttpCookie userCookie = new HttpCookie("UserName");
                userCookie.Expires = DateTime.Now.AddDays(-1); // Đặt thời gian hết hạn vào quá khứ
                Response.Cookies.Add(userCookie); // Thêm cookie vào response để xóa nó


                // Lưu thông tin vào session
                Session["UserId"] = user.Id;
                Session["Username"] = user.Username;

                // Lấy vai trò người dùng
                var userRole = db.UserRoles.FirstOrDefault(u => u.UserId == user.Id);
                Session["Role"] = userRole?.Role;

                Session.Timeout = 60; // Đặt thời gian session

                // Chuyển hướng theo vai trò
                if (userRole?.Role == "admin")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }
                else if (userRole?.Role == "client")
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }
        }

        return View(model);
    }


    public ActionResult Logout()
    {
        // Xóa các session
        Session.Clear();
        Session.Abandon();
        FormsAuthentication.SignOut();

        // Điều hướng người dùng về trang đăng nhập
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public ActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user != null)
            {
                // Tạo mã OTP
                var otp = GenerateOtp();
                var otpRecord = new OtpRecord
                {
                    Email = model.Email,
                    Otp = otp,
                    CreatedAt = DateTime.Now,
                    ExpiresAt = DateTime.Now.AddMinutes(5) // Mã OTP có hiệu lực trong 5 phút
                };

                // Lưu mã OTP vào cơ sở dữ liệu
                db.OtpRecords.Add(otpRecord);
                await db.SaveChangesAsync(); // Đừng quên await ở đây

                // Gửi email với mã OTP
                await _emailService.SendOtpEmailAsync(model.Email, otp);

                // Lưu email để dùng trong bước tiếp theo
                TempData["Email"] = model.Email;

                return RedirectToAction("VerifyOtp");
            }

            ModelState.AddModelError("", "Email không tồn tại.");
        }

        return View(model);
    }

    [HttpGet]
    public ActionResult VerifyOtp()
    {
        var email = TempData["Email"] as string; // Lấy email đã lưu trong TempData
        if (string.IsNullOrEmpty(email))
        {
            return RedirectToAction("ForgotPassword");
        }

        var model = new VerifyOtpViewModel
        {
            Email = email // Gán email vào model
        };

        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> VerifyOtp(VerifyOtpViewModel model)
    {
        if (ModelState.IsValid)
        {
            var otpRecord = await db.OtpRecords.FirstOrDefaultAsync(r => r.Email == model.Email && r.Otp == model.Otp);

            // Kiểm tra xem mã OTP có tồn tại và chưa hết hạn
            if (otpRecord != null && otpRecord.ExpiresAt > DateTime.Now)
            {
                // Xóa mã OTP sau khi đã xác minh
                db.OtpRecords.Remove(otpRecord);
                await db.SaveChangesAsync();

                return RedirectToAction("ResetPassword", new { email = model.Email });
            }
            else
            {
                ModelState.AddModelError("", "Mã OTP không chính xác hoặc đã hết hạn.");
            }
        }
        return View(model);
    }

    [HttpGet]
    public ActionResult ResetPassword(string email)
    {
        var model = new ResetPasswordViewModel { Email = email };
        return View(model);
    }

    [HttpPost]
    public ActionResult ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Tìm người dùng theo email
        var user = db.Users.FirstOrDefault(u => u.Email == model.Email);
        if (user == null)
        {
            ModelState.AddModelError("", "Không tìm thấy người dùng với email này.");
            return View(model);
        }

        // Mã hóa mật khẩu mới
        string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

        // Cập nhật mật khẩu
        user.PasswordHash = newPasswordHash;
        db.SaveChanges(); // Lưu thay đổi
        ViewBag.Message = "Mật khẩu đã được cập nhật thành công!";
        return View("Login"); // Redirect to a success page
    }


    [HttpGet]
    public ActionResult CreateProfile()
    {
        var model = new ProfileViewModel();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult CreateProfile(ProfileViewModel model)
    {
        if (ModelState.IsValid)
        {
            DateTime? dateOfBirth = model.DateOfBirth.HasValue ? model.DateOfBirth.Value.Date : (DateTime?)null;

            var newProfile = new Profile
            {
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                DateOfBirth = dateOfBirth, // Chỉ lưu phần ngày
                Gender = model.Gender,
                UserId = (int)(Session)["UserId"]
            };

            db.Profiles.Add(newProfile);
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }
        return View(model);
    }

    public ActionResult OrderHistory()
    {
        if (Session["UserId"] == null)
        {
            return RedirectToAction("Login", "Account");
        }
        int userId = (int)Session["UserId"];
        var orders = db.Orders.Where(o => o.UserID == userId).ToList();
        // Tạo model từ thông tin người dùng
        var model = new ProfileViewModel
        {
            Order = orders
        };
        return View(model);
    }


    [HttpGet]
    public ActionResult EditProfile()
    {
        if (Session["UserId"] == null)
        {
            return RedirectToAction("Login", "Account");
        }

        int userId = (int)Session["UserId"];
        var userProfile = db.Profiles.FirstOrDefault(p => p.UserId == userId);

        // Kiểm tra xem userProfile có tồn tại không
        if (userProfile == null)
        {
            return RedirectToAction("CreateProfile");
        }

        var model = new ProfileViewModel
        {
            UserId = (int)userProfile.UserId, // Gán UserId cho model
            FullName = userProfile.FullName,
            PhoneNumber = userProfile.PhoneNumber,
            Address = userProfile.Address,
            DateOfBirth = userProfile.DateOfBirth,
            Gender = userProfile.Gender
        };
        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult EditProfile(ProfileViewModel model)
    {
        if (Session["UserId"] == null)
        {
            return RedirectToAction("Login", "Account");
        }

        int userId = (int)Session["UserId"];

        if (ModelState.IsValid)
        {
            var userProfile = db.Profiles.FirstOrDefault(p => p.UserId == userId);
            if (userProfile != null)
            {
                // Cập nhật các thông tin từ model vào userProfile
                userProfile.FullName = model.FullName;
                userProfile.PhoneNumber = model.PhoneNumber;
                userProfile.Address = model.Address;
                userProfile.DateOfBirth = model.DateOfBirth;
                userProfile.Gender = model.Gender;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin người dùng.");
            }
        }

        return View(model);
    }


    private string GenerateOtp()
    {
        // Tạo mã OTP ngẫu nhiên 6 chữ số
        Random random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    public ActionResult Index()
    {
        // Kiểm tra xem session có tồn tại hay không
        if (Session["UserId"] == null)
        {
            // Chuyển hướng về trang đăng nhập nếu session không tồn tại
            return RedirectToAction("Login", "Account");
        }

        int userId = (int)Session["UserId"];
        // Tìm kiếm thông tin người dùng dựa trên UserId
        var userProfile = db.Profiles.FirstOrDefault(p => p.UserId == userId);

        //Kiểm tra xem người dùng có tồn tại không
        if (userProfile == null)
        {
            return RedirectToAction("CreateProfile", "Account");
        }

        // Tạo model từ thông tin người dùng
        var model = new ProfileViewModel
        {
            UserId = (int)userProfile.UserId,
            FullName = userProfile.FullName,
            PhoneNumber = userProfile.PhoneNumber,
            Address = userProfile.Address,
            DateOfBirth = userProfile.DateOfBirth,
            Gender = userProfile.Gender,
        };

        return View(model);
    }
}
