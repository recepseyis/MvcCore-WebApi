using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebUI.ViewModels
{
    public class PasswordChangeViewModel
    {
        [Display(Name = "Eski Şifreniz")]
        [Required(ErrorMessage = "Eski Şifreniz Gereklidir.")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Şifreniz En az 4 Karakterli Olmalıdır")]
        public string PasswordOld { get; set; }

        [Display(Name = "Yeni Şifreniz")]
        [Required(ErrorMessage = "Yeni Şifreniz Gereklidir.")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Şifreniz En az 4 Karakterli Olmalıdır")]
        public string PasswordNew { get; set; }

        [Display(Name = "Tekrar Yeni Şifreniz")]
        [Required(ErrorMessage = "Yeni Şifre Tekrarı Gereklidir.")]
        [DataType(DataType.Password)]
        [MinLength(4, ErrorMessage = "Şifreniz En az 4 Karakterli Olmalıdır")]
        [Compare("PasswordNew",ErrorMessage ="Yeni Şifre ile Tekrarı Uyuşmuyor.")]
        public string PasswordConfirm { get; set; }
    }
}
