using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebUI.ViewModels
{
    public class RoleViewModel
    {
        public string Id { get; set; }
        [Display(Name ="Role Adı")]
        [Required(ErrorMessage ="Role Adı Gereklidir.")]
        public string Name { get; set; }
    }
}
