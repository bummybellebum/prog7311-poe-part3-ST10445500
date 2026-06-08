using System.Security.Claims;
using GLMS.Web.ApiClients;
using GLMS.Web.ApiClients.Models;
using GLMS.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//ST10445500 - PROG7311 - GLMS POE
//AccountController

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthApiClient _authApiClient;

        public AccountController(IAuthApiClient authApiClient)
        {
            _authApiClient = authApiClient;
        }

        //........................................................................................//

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToLocal(returnUrl);
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        //........................................................................................//

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _authApiClient.LoginAsync(new LoginRequestDto
            {
                Email = vm.Email,
                Password = vm.Password,
                RememberMe = vm.RememberMe,
                ReturnUrl = vm.ReturnUrl
            });

            if (result.IsSuccess && result.Data != null && !string.IsNullOrWhiteSpace(result.Data.Token))
            {
                await SignInAsync(result.Data, vm.RememberMe);
                return RedirectToLocal(vm.ReturnUrl);
            }

            AddError(result);
            return View(vm);
        }

        //........................................................................................//

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        //........................................................................................//

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        //........................................................................................//

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var result = await _authApiClient.GetCurrentAccountAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                return Challenge();
            }

            return View(new ProfileViewModel
            {
                FirstName = result.Data.FirstName,
                LastName = result.Data.LastName,
                Email = result.Data.Email
            });
        }

        //........................................................................................//

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _authApiClient.UpdateCurrentAccountAsync(new UpdateAccountProfileDto
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email
            });

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Profile updated successfully.";
                return RedirectToAction(nameof(Profile));
            }

            AddError(result);
            return View(vm);
        }

        //........................................................................................//

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        //........................................................................................//

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var result = await _authApiClient.ChangePasswordAsync(new ChangePasswordRequestDto
            {
                CurrentPassword = vm.CurrentPassword,
                NewPassword = vm.NewPassword,
                ConfirmPassword = vm.ConfirmPassword
            });

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Password changed successfully.";
                return RedirectToAction(nameof(Profile));
            }

            AddError(result);
            return View(vm);
        }

        //........................................................................................//

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        //........................................................................................//

        private Task SignInAsync(AuthResponseDto auth, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, auth.UserId),
                new(ClaimTypes.Name, auth.Email),
                new(ClaimTypes.Email, auth.Email),
                new("ApiToken", auth.Token)
            };

            foreach (var role in auth.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var properties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = auth.ExpiresAt
            };

            return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
        }

        //........................................................................................//

        private void AddError(ApiClientResult result)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "The request could not be completed.");
        }

        //........................................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
