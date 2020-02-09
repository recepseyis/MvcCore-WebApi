using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Server.Kestrel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebUI.CustomValidation;
using WebUI.Models;
using WebUI.Models.Context;
using WebUI.RequreClasses;

namespace WebUI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication()
                .AddFacebook(x =>
                {
                    x.AppId = Configuration["Authentication:Facebook:AppId"];
                    x.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                    x.Fields.Add("picture");
                }).AddGoogle(x =>
                {
                    x.ClientId = Configuration["Authentication:Google:ClientID"];
                    x.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                }).AddMicrosoftAccount(x=> 
                { 
                    x.ClientId = Configuration["Authentication:Microsoft:ClientID"];
                    x.ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"];
                });
            /*
                 }).AddTwitter(x =>
                {
                    x.ConsumerKey = Configuration["Authentication:Twitter:ConsumerAPIKey"];
                    x.ConsumerSecret = Configuration["Authentication:Twitter:ConsumerSecret"];
                });
             */
            services.AddDbContext<AppDbContext>(opts =>
            {
                opts.UseSqlServer(Configuration["ConnectionStings:DefaultConnectionString"]);
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
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            int? httpsPort = null;
            var statusCode = env.IsDevelopment() ? StatusCodes.Status302Found : StatusCodes.Status301MovedPermanently;
            app.UseRewriter(new RewriteOptions().AddRedirectToHttps(statusCode, httpsPort));
            app.UseDeveloperExceptionPage();
            app.UseStatusCodePages();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                    
                endpoints.MapRazorPages();
            });
        }
    }
}
