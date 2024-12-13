using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ClothesStore.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="Tài khoản là bắt buộc")]
        public string Username { get; set; }

        [Required(ErrorMessage ="Mật khẩu là bắt buộc")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
    }
}