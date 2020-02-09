using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models;
using WebUI.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Authorization;

namespace WebUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : BaseController
    {
        public AdminController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager) : base(userManager, null, roleManager)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Claims()
        {
            return View(User.Claims.ToList());
        }

        public IActionResult RoleCreate()
        {
            return View();
        }
        [HttpPost]
        public IActionResult RoleCreate(RoleViewModel model)
        {
            AppRole role = new AppRole();
            role.Name = model.Name;
            IdentityResult result = roleManager.CreateAsync(role).Result;
            if (result.Succeeded)
            {
                return RedirectToAction("Roles");
            }
            else
            {
                AddModelError(result);
            }
            return View(model);
        }

        public IActionResult Roles()
        {
            return View(roleManager.Roles.ToList());
        }

        public IActionResult Users()
        {
            return View(userManager.Users.ToList());
        }

        public IActionResult RoleDelete(string Id)
        {
            AppRole role = roleManager.FindByIdAsync(Id).Result;
            IdentityResult result = roleManager.DeleteAsync(role).Result;
            return RedirectToAction("Roles");
        }

        public IActionResult RoleUpdate(string Id)
        {
            AppRole role = roleManager.FindByIdAsync(Id).Result;
            return View(role.Adapt<RoleViewModel>());
        }
        [HttpPost]
        public IActionResult RoleUpdate(RoleViewModel model)
        {
            AppRole role = roleManager.FindByIdAsync(model.Id).Result;
            role.Name = model.Name;
            IdentityResult result = roleManager.UpdateAsync(role).Result;
            return RedirectToAction("Roles");
        }
        public IActionResult RoleAssign (string Id)
        {
            TempData["userId"] = Id;
            AppUser user = userManager.FindByIdAsync(Id).Result;
            ViewBag.username = user.UserName;
            IQueryable<AppRole> roles = roleManager.Roles;
            List<string> userroles = userManager.GetRolesAsync(user).Result as List<string>;
            List<RoleAssignViewModel> roleAssignViewModels = new List<RoleAssignViewModel>();
            foreach (var role in roles)
            {
                RoleAssignViewModel r = new RoleAssignViewModel();
                r.RoleId = role.Id;
                r.RoleName = role.Name;
                if (userroles.Contains(role.Name))
                {
                    r.ischecked = true;
                }
                else
                {
                    r.ischecked = false;
                }
                roleAssignViewModels.Add(r);
            }
            return View(roleAssignViewModels);
        }
        [HttpPost]
        public async Task<IActionResult> RoleAssign(List<RoleAssignViewModel> model)
        {
            AppUser user = userManager.FindByIdAsync(TempData["userId"].ToString()).Result;
            foreach (var item in model)
            {
                if (item.ischecked)
                {
                   await userManager.AddToRoleAsync(user, item.RoleName);
                }
                else
                {
                   await userManager.RemoveFromRoleAsync(user, item.RoleName);
                }
            }
            return RedirectToAction("Users");
        }


        public async Task<IActionResult> ResetUserPassword(string Id)
        {
            AppUser user = await userManager.FindByIdAsync(Id);
            PasswordResetAdminByUserViewModel passadmin = new PasswordResetAdminByUserViewModel();
            passadmin.UserId = user.Id;
            return View(passadmin);
        }
        [HttpPost]
        public async Task<IActionResult> ResetUserPassword(PasswordResetAdminByUserViewModel model)
        {
            AppUser user = await userManager.FindByIdAsync(model.UserId);
            string token = await userManager.GeneratePasswordResetTokenAsync(user);
            await userManager.ResetPasswordAsync(user, token, model.NewPassword);
            await userManager.UpdateSecurityStampAsync(user);
            await userManager.GetLockoutEnabledAsync(user);
            return RedirectToAction("Users");
        }

    }
}