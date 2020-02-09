using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebUI.Models;

namespace WebUI.ClaimProvider
{
    public class ClaimProvider : IClaimsTransformation
    {
        public UserManager<AppUser> userManager { get; set; }
        public ClaimProvider(UserManager<AppUser> _userManager)
        {
            this.userManager = _userManager;
        }
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal != null && principal.Identity.IsAuthenticated)
            {
                ClaimsIdentity identity = principal.Identity as ClaimsIdentity;
                AppUser user = await userManager.FindByNameAsync(identity.Name);
                if (user != null)
                {
                    if (user.BirthDay != null)
                    {
                        var today = DateTime.Today;
                        var age = today.Year - user.BirthDay?.Year;
                        bool status = false;
                        if (age > 15)
                        {
                            Claim ViolanceClaim = new Claim("Violance", true.ToString(), ClaimValueTypes.String, "Internal");
                            identity.AddClaim(ViolanceClaim);
                        }
                    }

                    if (user.City != null)
                    {
                        if (!principal.HasClaim(c => c.Type == "city"))
                        {
                            Claim CityClaim = new Claim("city", user.City, ClaimValueTypes.String, "Internal");
                            identity.AddClaim(CityClaim);
                        }
                    }
                }
            }
            return principal;
        }
    }
}
