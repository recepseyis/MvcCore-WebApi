﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebUI.Models;

namespace WebUI.CustomValidation
{
    public class CustomPasswordValidator : IPasswordValidator<AppUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string password)
        {
            List<IdentityError> errors = new List<IdentityError>();
            if (password.ToLower().Contains(user.UserName.ToLower()))
            {
                if (user.Email.Contains(user.UserName))
                {
                    errors.Add(new IdentityError() { Code = "PasswordContainsUserName", Description = "Şifre Alanı Kullanıcı Adı İçeremez." });
                }
            }
            if (password.ToLower().Contains("1234"))
            {
                errors.Add(new IdentityError() { Code = "PasswordContains1234", Description = "Şifre Alanı Ardışık Sayı İçeremez." });
            }
            if (password.ToLower().Contains(user.Email.ToLower()))
            {
                errors.Add(new IdentityError() { Code = "PasswordContainsEmail", Description = "Şifre Alanı Email İçeremez." });
            }
            if (errors.Count == 0)
            {
                return Task.FromResult(IdentityResult.Success);
            }
            else
            {
                return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
            }
        }
    }
}
