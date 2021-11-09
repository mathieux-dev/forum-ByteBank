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

                    return userManager;
                });
        }
    }
}