using ByteBank.Forum.Models;
using ByteBank.Forum.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ByteBank.Forum.Controllers
{
    public class ContaController : Controller
    {
        private UserManager<UserApplication> _userManager;
        public UserManager<UserApplication> UserManager
        {
            get
            {
                if(_userManager == null)
                {
                    var contextOwin = HttpContext.GetOwinContext();
                    _userManager = contextOwin.GetUserManager<UserManager<UserApplication>>();
                }
                return _userManager;
            }
            set
            {
                _userManager = value;
            }
        }

        public ActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Registrar(AccountRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newUser = new UserApplication();

                newUser.Email = model.Email;
                newUser.UserName = model.UserName;
                newUser.NomeCompleto = model.NomeCompleto;

                var user = UserManager.FindByEmail(newUser.Email);
                var userAlreadyExists = user != null;

                if (userAlreadyExists)
                    return RedirectToAction("Index", "Home");

                var result = await UserManager.CreateAsync(newUser, model.Senha);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    AddErrors(result);
                }
            }

            return View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach(var error in result.Errors)
                ModelState.AddModelError("", error);
        }
    }
}