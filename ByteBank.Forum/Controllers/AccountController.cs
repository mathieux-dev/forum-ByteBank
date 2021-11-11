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
using System.Net;
using Microsoft.Owin.Security;

namespace ByteBank.Forum.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<UserApplication> _userManager;
        public UserManager<UserApplication> UserManager
        {
            get
            {
                if (_userManager == null)
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

        private SignInManager<UserApplication, string> _signInManager;
        public SignInManager<UserApplication, string> SignInManager
        {
            get
            {
                if (_signInManager == null)
                {
                    var contextOwin = HttpContext.GetOwinContext();
                    _signInManager = contextOwin.GetUserManager<SignInManager<UserApplication, string>>();
                }
                return _signInManager;
            }
            set
            {
                _signInManager = value;
            }
        }

        public IAuthenticationManager AuthenticationManager
        {
            get
            {
                var contextOwin = Request.GetOwinContext();
                return contextOwin.Authentication;
            }
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(AccountRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newUser = new UserApplication();

                newUser.Email = model.Email;
                newUser.UserName = model.UserName;
                newUser.NomeCompleto = model.NomeCompleto;

                var user = await UserManager.FindByEmailAsync(newUser.Email);
                var userAlreadyExists = user != null;

                if (userAlreadyExists)
                    return View("AwaitingConfirmation");

                var result = await UserManager.CreateAsync(newUser, model.Senha);

                if (result.Succeeded)
                {
                    await SendConfirmationEmailAsync(newUser);
                    return View("AwaitingConfirmation");
                }
                else
                {
                    AddErrors(result);
                }
            }

            return View(model);
        }

        private async Task SendConfirmationEmailAsync(UserApplication user)
        {
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);

            var callbackLink =
                Url.Action(
                    "EmailConfirmation",
                    "Conta",
                    new { userId = user.Id, token = token },
                    Request.Url.Scheme);

            await UserManager.SendEmailAsync(
                user.Id,
                "Fórum ByteBank - Confirmação de E-mail",
                $"Bem vindo ao fórum ByteBank, clique aqui: {callbackLink} para confirmar seu endereço de e-mail.");
        }

        public async Task<ActionResult> EmailConfirmation(string userId, string token)
        {
            if (userId == null || token == null)
                return View("Error");

            var result = await UserManager.ConfirmEmailAsync(userId, token);

            if (result.Succeeded)
                return View("Index", "Home");
            else
                return View("Error");
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(AccountLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);

                if (user == null)
                    return UserOrPasswordInvalid();

                var signInResult =
                    await SignInManager.PasswordSignInAsync(
                        user.UserName,
                        model.Password,
                        isPersistent: true,
                        shouldLockout: true);

                switch (signInResult)
                {
                    case SignInStatus.Success:
                        if (!user.EmailConfirmed)
                        {
                            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                            return View("AwaitingConfirmation");
                        }
                        return RedirectToAction("Index", "Home");

                    case SignInStatus.LockedOut:
                        var correctPassword =
                            await UserManager.CheckPasswordAsync(
                                user,
                                model.Password);
                        if (correctPassword)
                            ModelState.AddModelError("", "A conta está bloqueada.");
                        else
                            return UserOrPasswordInvalid();
                        break;

                    default:
                        return UserOrPasswordInvalid();
                }
            }

            return View();
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(AccountForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    var token = UserManager.GeneratePasswordResetTokenAsync(user.Id);

                    var callbackLink =
                        Url.Action(
                            "PasswordChangeConfirmation",
                            "Account",
                            new { userId = user.Id, token = token },
                            Request.Url.Scheme);

                    await UserManager.SendEmailAsync(
                        user.Id,
                        "Fórum ByteBank - Alteração de senha",
                        $"Clique aqui: {callbackLink} para alterar sua senha.");
                }

                return View("PasswordChangeEmailSent");
            }

            return View();
        }

        public ActionResult PasswordChangeConfirmation(string userId, string token)
        {
            var model = new AccountPasswordChangeConfirmationViewModel
            {
                UserId = userId,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> PasswordChangeConfirmation(AccountPasswordChangeConfirmationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var resultChange = 
                    await UserManager.ResetPasswordAsync(
                    model.UserId,
                    model.Token,
                    model.NewPassword);

                if (resultChange.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                AddErrors(resultChange);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            return RedirectToAction("Index", "Home");
        }

        private ActionResult UserOrPasswordInvalid()
        {
            ModelState.AddModelError("", "Credenciais inválidas");

            return View("Login");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error);
        }
    }
}