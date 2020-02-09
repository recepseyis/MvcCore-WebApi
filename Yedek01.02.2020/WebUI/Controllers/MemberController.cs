using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebUI.Models;
using WebUI.ViewModels;
using Mapster;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebUI.Enums;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace WebUI.Controllers
{
    [Authorize]
    public class MemberController : BaseController
    {
        public MemberController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : base(userManager, signInManager)
        {
        }

        public IActionResult Index()
        {
            AppUser user = CurrrentUser;
            UserViewModel userViewModel = user.Adapt<UserViewModel>();
            return View(userViewModel);
        }

        public IActionResult UserEdit()
        {
            AppUser user = CurrrentUser;

            UserViewModel model = user.Adapt<UserViewModel>();

            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserViewModel model, IFormFile userPicture)
        {
            ModelState.Remove("Password");
            ViewBag.Gender = new SelectList(Enum.GetNames(typeof(Gender)));
            if (ModelState.IsValid)
            {
                AppUser user = CurrrentUser;
                string phone = userManager.GetPhoneNumberAsync(user).Result;
                if (phone != model.PhoneNumber)
                {
                    if (userManager.Users.Any(x => x.PhoneNumber == model.PhoneNumber))
                    {
                        ModelState.AddModelError("", "Bu Telefon Numarası Daha Önceden Kayıtlı");
                        return View(model);
                    }
                }
                if (userPicture != null && userPicture.Length > 0)
                {
                    var userfilename = user.UserName + Guid.NewGuid().ToString() + Path.GetExtension(userPicture.FileName);
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/UserPicture", userfilename);
                    using (var steam = new FileStream(path, FileMode.Create))
                    {
                        await userPicture.CopyToAsync(steam);
                        user.UserPicture = "/UserPicture/" + userfilename;
                    }
                }
                user.UserName = model.UserName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.City = model.City;
                user.BirthDay = model.BirthDay;
                user.GenderN = (int)model.Gender;
                IdentityResult result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await userManager.UpdateSecurityStampAsync(user);
                    await signInManager.SignOutAsync();
                    await signInManager.SignInAsync(user, true);
                    ViewBag.success = true;
                }
                else
                {
                    AddModelError(result);
                }
            }

            return View(model);
        }

        public IActionResult PasswordChange()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PasswordChange(PasswordChangeViewModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = CurrrentUser;
                bool exits = userManager.CheckPasswordAsync(user, model.PasswordOld).Result;
                if (exits)
                {
                    IdentityResult result = userManager.ChangePasswordAsync(user, model.PasswordOld, model.PasswordNew).Result;
                    if (result.Succeeded)
                    {
                        userManager.UpdateSecurityStampAsync(user);
                        signInManager.SignOutAsync();
                        signInManager.PasswordSignInAsync(user, model.PasswordNew, true, false);
                        ViewBag.success = true;
                    }
                    else
                    {
                        AddModelError(result);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Eski Şifreniz Yanlış");
                }
            }
            return View(model);
        }

        public void LogOut()
        {
            signInManager.SignOutAsync();
        }

        public IActionResult AccessDenied(string ReturnUrl)
        {
            if (ReturnUrl.Contains("ViolancePage"))
            {
                ViewBag.message = "Erişilmeye çalışılan sayfa şiddet içeriyor yaş 15+ olması gerekir";
            }
            else if (ReturnUrl.Contains("AnkaraPage"))
            {
                ViewBag.message = "Şehriniz Ankara Değilse Giremezsiniz.";
            }
            else if (ReturnUrl.Contains("Exchange"))
            {
                ViewBag.message = "30 Günlük Ücretsiz Kullanım Süresi Dolmuştur.";
            }
            else
            {
                ViewBag.message = "Bu Sayfaya Erişim İzniniz Yoktur.";
            }
            return View();
        }

        [Authorize(Roles = "Editor")]
        public IActionResult Editor()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Manager() => View();
        [Authorize(Policy = "AnkaraPolicy")]
        public IActionResult AnkaraPage() => View();
        [Authorize(Policy = "ViolancePolicy")]
        public IActionResult ViolancePage() => View();
        public async Task<IActionResult> ExchangeRedirect()
        {
            bool result = User.HasClaim(x => x.Type == "ExpireDateExchange");
            if (!result)
            {
                Claim ExpireDateExchange = new Claim("ExpireDateExchange",DateTime.Now.AddDays(30).Date.ToShortDateString(),ClaimValueTypes.String,"Internal");
                await userManager.AddClaimAsync(CurrrentUser, ExpireDateExchange);
                await signInManager.SignOutAsync();
                await signInManager.SignInAsync(CurrrentUser, true);
            }
            return RedirectToAction("Exchange");
        }
        [Authorize(Policy = "ExchangePolicy")]
        public IActionResult Exchange() => View();
    }
}