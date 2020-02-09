using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebUI.Enums;

namespace WebUI.ViewModels
{
    public class UserViewModel
    {
        [Required(ErrorMessage ="Kullanıcı Adı Gereklidir.")]
        [Display(Name ="Kullanıcı Adı")]
        public string UserName { get; set; }

        [Display(Name = "Telefon")]
        [RegularExpression(@"^(0(\d{3}) (\d{3}) (\d{2}) (\d{2}))$", ErrorMessage ="Format Doğru Değil")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email Gereklidir.")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage ="Geçerli Değil")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre Gereklidir.")]
        [Display(Name = "Şifre")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Doğum Günün")]
        [DataType(DataType.Date)]
        public DateTime? BirthDay { get; set; }
        [Display(Name = "Fotoğrafın")]
        public string UserPicture { get; set; }
        [Display(Name = "Şehrin")]
        public string City { get; set; }
        [Display(Name = "Cinsiyet")]
        public Gender Gender { get; set; }
    }
}
