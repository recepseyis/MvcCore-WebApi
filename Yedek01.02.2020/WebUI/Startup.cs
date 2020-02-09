using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebUI.CustomValidation;
using WebUI.Models;
using WebUI.Models.Context;
using WebUI.RequreClasses;

namespace WebUI
{
    public class Startup
    {
        public IConfiguration configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication()
                .AddFacebook(x =>
                {
                    x.AppId = configuration["Authentication:Facebook:AppId"];
                    x.AppSecret = configuration["Authentication:Facebook:AppSecret"];
                    x.Fields.Add("picture");
                }).AddGoogle(x =>
                {
                    x.ClientId = configuration["Authentication:Google:ClientID"];
                    x.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                }).AddMicrosoftAccount(x=> 
                { 
                    x.ClientId = configuration["Authentication:Microsoft:ClientID"];
                    x.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
                });
            services.AddDbContext<AppDbContext>(opts =>
            {
                opts.UseSqlServer(configuration["ConnectionStings:DefaultConnectionString"]);
            });

            services.AddIdentity<AppUser, AppRole>(opts =>
            {
                opts.User.RequireUniqueEmail = true;
                opts.User.AllowedUserNameCharacters = "abcçdefğghıijklmnoöpqrsştuüvwxyzABCÇDEFGĞHİIJKLMNOÖPQRŞSTUVWXYZ0123456789-._/";
                opts.Password.RequiredLength = 4;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireDigit = false;
            }).AddPasswordValidator<CustomPasswordValidator>
            ().AddErrorDescriber<CustomIdentityErrorDescriber>
            ().AddUserValidator<CustomUserValidator>
            ().AddEntityFrameworkStores<AppDbContext>
            ().AddDefaultTokenProviders();

            CookieBuilder cookieBuilder = new CookieBuilder();
            cookieBuilder.Name = "RsYazilim";
            cookieBuilder.HttpOnly = false;
            cookieBuilder.SameSite = SameSiteMode.Lax;
            cookieBuilder.SecurePolicy = CookieSecurePolicy.SameAsRequest;


            services.ConfigureApplicationCookie(opts =>
            {
                opts.LoginPath = new PathString("/Home/Login");
                opts.LogoutPath = new PathString("/Member/LogOut");
                opts.Cookie = cookieBuilder;
                opts.SlidingExpiration = true;
                opts.ExpireTimeSpan = System.TimeSpan.FromDays(365);
                opts.AccessDeniedPath = new PathString("/Member/AccessDenied");
            });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
            services.AddTransient<IAuthorizationHandler, ExpireDateExchangeHandle>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAdministratorRole",
                     policy => policy.RequireRole("Admin"));
            });
            services.AddAuthorization(opts =>
            {
                opts.AddPolicy("AnkaraPolicy", policy =>
                {
                    policy.RequireClaim("city", "Ankara");
                });
                opts.AddPolicy("ViolancePolicy", policy =>
                 {
                     policy.RequireClaim("Violance");
                 });
                opts.AddPolicy("ExchangePolicy", policy =>
                 {
                     policy.AddRequirements(new ExpireDateExchangeRequirement());
                 });
            });
            services.AddScoped<IClaimsTransformation, ClaimProvider.ClaimProvider>();
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseStatusCodePages();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}
