using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebUI.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name ="Email Adresi")]
        [Required(ErrorMessage ="Email Alanı Gereklidir.")]
        [EmailAddress]
        public string Email { get; set; }
        [Display(Name = "Şifre Alanı")]
        [Required(ErrorMessage = "Şifre Gereklidir.")]
        [DataType(DataType.Password)]
        [MinLength(4,ErrorMessage ="Şifreniz En az 4 Karakter Olmalıdır.")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
