using ByteBank.Forum.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using ByteBank.Forum.App_Start.Identity;
using Microsoft.Owin.Security.Cookies;

[assembly: OwinStartup(typeof(ByteBank.Forum.Startup))]

namespace ByteBank.Forum
{
    public class Startup
    {
        public void Configuration(IAppBuilder builder)
        {
            builder.CreatePerOwinContext<DbContext>(() =>
                new IdentityDbContext<UserApplication>("DefaultConnection"));

            builder.CreatePerOwinContext<IUserStore<UserApplication>>(
                (options, contextOwin) =>
                {
                    var dbContext = contextOwin.Get<DbContext>();
                    return new UserStore<UserApplication>(dbContext);
                });

            builder.CreatePerOwinContext<UserManager<UserApplication>>(
                (options, contextOwin) =>
                {
                    var userStore = contextOwin.Get<IUserStore<UserApplication>>();
                    var userManager = new UserManager<UserApplication>(userStore);

                    var userValidator = new UserValidator<UserApplication>(userManager);
                    userValidator.RequireUniqueEmail = true;

                    userManager.UserValidator = userValidator;
                    userManager.PasswordValidator = new ValidatePassword()
                    {
                        RequiredLength = 8,
                        RequireSpecialCharacter = true,
                        RequireLowerCaseCharacter = true,
                        RequireUpperCaseCharacter = true,
                        RequireDigit = true
                    };

                    userManager.EmailService = new EmailService();

                    var dataProtectionProvider = options.DataProtectionProvider;
                    var dataProtectionProviderCreated = dataProtectionProvider.Create("ByteBank.Forum");

                    userManager.UserTokenProvider = new DataProtectorTokenProvider<UserApplication>(dataProtectionProviderCreated);

                    userManager.MaxFailedAccessAttemptsBeforeLockout = 5;
                    userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    userManager.UserLockoutEnabledByDefault = true;

                    return userManager;
                });

            builder.CreatePerOwinContext<SignInManager<UserApplication, string>>(
                (options, contextOwin) =>
                {
                    var userManager = contextOwin.Get<UserManager<UserApplication>>();
                    var signInManager =
                        new SignInManager<UserApplication, string>(
                            userManager,
                            contextOwin.Authentication);

                    return signInManager;
                });

            builder.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie
            });
        }
    }
}