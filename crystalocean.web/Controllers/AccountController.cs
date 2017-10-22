using System;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CrystalOcean.Data.Models;
using CrystalOcean.Web.Extensions;
using CrystalOcean.Web.Models;
using CrystalOcean.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CrystalOcean.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly RoleManager<Role> roleManager;
        private readonly ILogger<AccountController> logger;
        private readonly IEmailSender emailSender;

        public AccountController(UserManager<User> userManager,
                                SignInManager<User> signInManager,
                                RoleManager<Role> roleManager,
                                IEmailSender emailSender,
                                ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.emailSender = emailSender;
            this.logger = logger;
        }

        [TempData]
        public string ErrorMessage { get; set; }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(String returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            logger.LogInformation("User logged out.");
            return RedirectToLocal(null);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(String returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult EmailConfirmation()
        {
            return View();
        }

        private async Task<bool> EnsureUserRoleAsync()
        {
            bool roleExists = await roleManager.RoleExistsAsync(Role.USER);
            if(!roleExists)
            {
                var role = new Role();
                role.Name = Role.USER;
                role.Description = "The role can perform normal operations.";
                var result = await roleManager.CreateAsync(role);
                return result.Succeeded;
            }
            return true;
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, String returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var roleCreated = await EnsureUserRoleAsync();
                if (!roleCreated)
                {
                    AddError("Error occurred while creating a role.");
                    return View(model);
                }

                var user = new User() { UserName = model.Email, Email = model.Email,
                    FirstName = model.FirstName, LastName = model.LastName,
                    Prefix = model.Prefix, Gender = model.Gender };

                var result = await userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    result = await userManager.AddToRoleAsync(user, Role.USER);
                }
                
                if (result.Succeeded)
                {
                    logger.LogInformation("New account has been created with password: email=({EMAIL}).", model.Email);

                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmUrl = Url.EmailConfirmationLink(user.Email, code, Request.Scheme);
                    
                    await SendEmailConfirmationAsync(model.Email, confirmUrl);
                    
                    return RedirectToAction(nameof(EmailConfirmation));
                }
                AddErrors(result);
            }

            // If we got this far something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, String returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    logger.LogInformation("User logged in : email=({EMAIL})", model.Email);
                    return RedirectToLocal(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    logger.LogWarning("User account locked out : email=({EMAIL}).", model.Email);
                    return RedirectToAction(nameof(Lockout));
                }
                
                ModelState.AddModelError(String.Empty, "Invalid login attempt.");
            }

            // If we got this far then something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(String email, String code)
        {
            if (email == null || code == null)
            {
                return RedirectToLocal(null);
            }
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with email '{email}'.");
            }
            var result = await userManager.ConfirmEmailAsync(user, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction(nameof(ForgotPasswordConfirmation));
                }

                // For more information on how to enable account confirmation and password reset please 
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await userManager.GeneratePasswordResetTokenAsync(user);
                var callbackUrl = Url.ResetPasswordCallbackLink(user.Email, code, Request.Scheme);
                
                await emailSender.SendEmailAsync(model.Email, 
                    "Reset password for CrystalOcean",
                    $"A request has been made to reset your password. You can change your password by clicking on the link below: <a href='{callbackUrl}'>{callbackUrl}</a>. This link will expire within 24 hours. If you did not request that your password be reset, please submit an inquiry to 'support@orpaltech.com'. Thank you, CrystalOcean Support Team");
                
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // If we got this far then something failed, redisplay form
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(String code = null)
        {
            if (code == null)
            {
                throw new ApplicationException("A code must be supplied for password reset.");
            }
            var model = new ResetPasswordViewModel { Code = code };
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            AddErrors(result);
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(String provider, String returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(String returnUrl = null, String remoteError = null)
        {
            if (remoteError != null)
            {
                this.ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToAction(nameof(Lockout));
            }

            // If the user does not have an account, then ask the user to create an account.
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            return View("ExternalLogin", new ExternalLoginViewModel{ Email = email });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginViewModel model, String returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var roleCreated = await EnsureUserRoleAsync();
                if (!roleCreated)
                {
                    AddError("Error occurred while creating a role.");
                }
                else
                {
                    // Get the information about the user from the external login provider
                    var info = await signInManager.GetExternalLoginInfoAsync();
                    if (info == null)
                    {
                        throw new ApplicationException("Error loading external login information during confirmation.");
                    }
                    // Check if email exists in the social network provider
                    var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                    var user = new User { UserName = model.Email, Email = model.Email,
                                        EmailConfirmed = (model.Email == email) };
            
                    var result = await userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        result = await userManager.AddToRoleAsync(user, Role.USER);
                        
                        if (result.Succeeded)
                        {
                            result = await userManager.AddLoginAsync(user, info);
                        }
                        
                        if (result.Succeeded)
                        {
                            logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                            /*if (model.Email == email)
                            {*/
                            await signInManager.SignInAsync(user, isPersistent: false);
                            return RedirectToLocal(returnUrl);
                            /*}
                            else
                            {
                                // confirm email 
                                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                                var confirmUrl = Url.EmailConfirmationLink(user.Email, code, Request.Scheme);
                                await SendEmailConfirmationAsync(model.Email, confirmUrl);
                                return RedirectToAction(nameof(EmailConfirmation));
                            }*/
                        }
                    }
                    AddErrors(result);
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(nameof(ExternalLogin), model);
        }

        private Task SendEmailConfirmationAsync(String email, String link)
        {
            return emailSender.SendEmailAsync(email, "Confirm your email for CrystalOcean",
                $"Please, confirm your account by clicking the link below: <a href='{HtmlEncoder.Default.Encode(link)}'>{link}</a>");
        }

        private void AddError(String description)
        {
            ModelState.AddModelError(String.Empty, description);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(String.Empty, error.Description);
        }

        private IActionResult RedirectToLocal(String returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}