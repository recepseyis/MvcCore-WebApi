using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WebUI.Models;
using WebUI.ViewModels;

namespace WebUI.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager) : base(userManager, signInManager)
        {

        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Member");
            }
            return View();
        }

        public IActionResult Login(string ReturnUrl)
        {
            TempData["ReturnUrl"] = ReturnUrl;

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await userManager.FindByEmailAsync(loginViewModel.Email);
                if (user != null)
                {
                    if (await userManager.IsLockedOutAsync(user))
                    {
                        ModelState.AddModelError("", "Hesabınız Bir Süreliğine Kitlenmiştir! Lütfen Daha Sonra Tekrar Deneyiniz.");
                        return View(loginViewModel);
                    }
                    if (!userManager.IsEmailConfirmedAsync(user).Result)
                    {
                        ModelState.AddModelError("", "Email Doğrulanmamıştır. Emailinizi Kontrol Ediniz");
                        return View(loginViewModel);
                    }

                    await signInManager.SignOutAsync();
                    Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(user, loginViewModel.Password, loginViewModel.RememberMe, false);//yanlış giriş kullanıcı kitle son false
                    if (result.Succeeded)
                    {
                        await userManager.ResetAccessFailedCountAsync(user);
                        if (TempData["ReturnUrl"] != null)
                        {
                            return Redirect(TempData["ReturnUrl"].ToString());
                        }
                        return RedirectToAction("Index", "Member");
                    }
                    else
                    {
                        await userManager.AccessFailedAsync(user);
                        int fail = await userManager.GetAccessFailedCountAsync(user);
                        //ModelState.AddModelError("", $"{fail} kez Başarısız Giriş");
                        if (fail == 3)
                        {
                            DateTime kitli = (DateTime.Now.AddMinutes(1));
                            await userManager.SetLockoutEndDateAsync(user, new System.DateTimeOffset(DateTime.Now.AddMinutes(1)));
                            ModelState.AddModelError("", $"Hesabınız {fail} Başarısız Giriş Nedeni İle  {kitli} 'ya Kadar Kitlenmiştir. Lütfen Daha Sonra Deneyiniz.");
                        }
                        else
                        {
                            ModelState.AddModelError("", ClientMesajlar.KullaniciMesajlar.KullaniciHataliGiris);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", ClientMesajlar.KullaniciMesajlar.EmailAdresiBulunamamistir);
                }
            }
            return View(loginViewModel);
        }

        public IActionResult SingUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SingUp(UserViewModel userViewModel)
        {
            if (ModelState.IsValid)
            {
                if (userManager.Users.Any(x=> x.PhoneNumber == userViewModel.PhoneNumber))
                {
                    ModelState.AddModelError("", "Bu Telefon Numarası Daha Önceden Kayıtlı");
                    return View(userViewModel);
                }
                AppUser user = new AppUser();
                user.UserName = userViewModel.UserName;
                user.Email = userViewModel.Email;
                user.PhoneNumber = userViewModel.PhoneNumber;
                IdentityResult result = await userManager.CreateAsync(user, userViewModel.Password);
                if (result.Succeeded)
                {
                    string confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    string link = Url.Action("ConfirmEmail", "Home", new
                    {
                        userId = user.Id,
                        token = confirmationToken,

                    }, protocol: HttpContext.Request.Scheme);
                    Helper.EmailConfirmation.EmailConfirmForSendEmail(user.Email, link);
                    return RedirectToAction("Login");
                }
                else
                {
                    AddModelError(result);
                }
            }
            return View(userViewModel);
        }
        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            AppUser user = userManager.FindByEmailAsync(model.Email).Result;
            if (user != null)
            {
                string passwordResetToken = userManager.GeneratePasswordResetTokenAsync(user).Result;
                string passwordresetlink = Url.Action("ResetPasswordConfirm", "Home", new
                {
                    userId = user.Id,
                    token = passwordResetToken
                }, HttpContext.Request.Scheme);
                Helper.PasswordReset.PasswordResetSendEmail(model.Email, passwordresetlink);
                ViewBag.status = "MailSendsuccess";
            }
            else
            {
                ModelState.AddModelError("", "Sistemde Kayıtlı Mail Adresi Yoktur.");
            }
            return View(model);
        }


        public IActionResult ResetPasswordConfirm(string userId, string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPasswordConfirm([Bind("PasswordNew")] ResetPasswordViewModel model)
        {
            string token = TempData["token"].ToString();
            string userId = TempData["userId"].ToString();
            AppUser user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                IdentityResult result = await userManager.ResetPasswordAsync(user, token, model.PasswordNew);
                if (result.Succeeded)
                {
                    await userManager.UpdateSecurityStampAsync(user);
                    ViewBag.status = "success";
                }
                else
                {
                    AddModelError(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "Bir Hata Oluştu.");
            }
            return View(model);
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await userManager.FindByIdAsync(userId);
            IdentityResult result = await userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                ViewBag.status = "Email Adresi Onaylanmıştır.";
            }
            else
            {
                ViewBag.status = "Bir Hata Oluştu.";
            }
            return View();
        }

        public IActionResult FacebookLogin(string ReturnUrl)
        {
            string redirecturl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties("Facebook", redirecturl);
            return new ChallengeResult("Facebook", properties);
        }
        public IActionResult GoogleLogin(string ReturnUrl)
        {
            string redirecturl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties("Google", redirecturl);
            return new ChallengeResult("Google", properties);
        }
        public IActionResult MicrosoftLogin(string ReturnUrl)
        {
            string redirecturl = Url.Action("ExternalResponse", "Home", new { ReturnUrl = ReturnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties("Microsoft", redirecturl);
            return new ChallengeResult("Microsoft", properties);
        }

        public async Task<IActionResult> ExternalResponse(string ReturnUrl = "/")
        {
            ExternalLoginInfo info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);
                if (result.Succeeded)
                {
                    return Redirect(ReturnUrl);
                }
                else
                {
                    AppUser user = new AppUser();
                    user.Email = info.Principal.FindFirst(ClaimTypes.Email).Value;
                    string ExternalUserId = info.Principal.FindFirst(ClaimTypes.NameIdentifier).Value;
                    if (info.Principal.HasClaim(x=> x.Type == ClaimTypes.Name))
                    {
                        string userName = info.Principal.FindFirst(ClaimTypes.Name).Value;
                        userName = userName.Replace(' ', '-').ToLower() + ExternalUserId.Substring(0, 2).ToString();
                        user.UserName = userName;
                    }
                    else
                    { 
                    user.UserName = info.Principal.FindFirst(ClaimTypes.Email).Value;
                    }
                    AppUser user2 = await userManager.FindByEmailAsync(user.Email);
                    if (user2== null)
                    {
                        IdentityResult createresult = await userManager.CreateAsync(user);
                        if (createresult.Succeeded)
                        {
                            IdentityResult loginresult = await userManager.AddLoginAsync(user, info);
                            if (loginresult.Succeeded)
                            {
                                //await signInManager.SignInAsync(user, true);
                                await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);
                                return Redirect(ReturnUrl);
                            }
                            else
                            {
                                AddModelError(loginresult);
                            }
                        }
                        else
                        {
                            AddModelError(createresult);
                        }
                    }
                    else
                    {
                        IdentityResult createresult = await userManager.CreateAsync(user2);
                        await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, true);
                        return Redirect(ReturnUrl);
                    }

                }
                List<string> errors = ModelState.Values.SelectMany(x => x.Errors).Select(y => y.ErrorMessage).ToList();

                return View("Error", errors);
            }
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}