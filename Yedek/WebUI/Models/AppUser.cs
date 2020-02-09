using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace WebUI.Models
{
    public class AppUser:IdentityUser
    {
        [MaxLength(50)]
        public string City { get; set; }
        public string UserPicture { get; set; }
        public DateTime? BirthDay { get; set; }
        public int GenderN { get; set; }
    }
}
