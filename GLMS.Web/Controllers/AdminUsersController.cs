using GLMS.Web.Security;
using GLMS.Web.Services;
using GLMS.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

//ST10445500 - PROG7311 - GLMS POE
//AdminUsersController

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Controllers
{
    [Authorize(Roles = ApplicationRoles.Admin)]
    [Route("Admin/Users")]
    public class AdminUsersController : Controller
    {
        private readonly IAccountService _accountService;

        public AdminUsersController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        //........................................................................................//

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var users = await _accountService.GetUsersAsync();
            return View(users);
        }

        //........................................................................................//

        [HttpGet("Create")]
        public IActionResult Create()
        {
            var vm = new AdminUserCreateViewModel();
            PopulateRoleOptions(vm);
            return View(vm);
        }

        //........................................................................................//

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminUserCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                PopulateRoleOptions(vm);
                return View(vm);
            }

            var result = await _accountService.CreateUserAsync(vm);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction(nameof(Index));
            }

            AddErrors(result);
            PopulateRoleOptions(vm);
            return View(vm);
        }

        //........................................................................................//

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var vm = await _accountService.GetUserForEditAsync(id);
            if (vm == null)
            {
                return NotFound();
            }

            PopulateRoleOptions(vm);
            return View(vm);
        }

        //........................................................................................//

        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, AdminUserEditViewModel vm)
        {
            if (id != vm.UserId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                PopulateRoleOptions(vm);
                return View(vm);
            }

            var result = await _accountService.UpdateUserAsync(vm);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            AddErrors(result);
            PopulateRoleOptions(vm);
            return View(vm);
        }

        //........................................................................................//

        [HttpPost("ResetPassword/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, AdminResetPasswordViewModel vm)
        {
            if (id != vm.UserId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please enter and confirm a valid temporary password.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var result = await _accountService.ResetPasswordAsync(vm);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Temporary password set successfully.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
            return RedirectToAction(nameof(Edit), new { id });
        }

        //........................................................................................//

        [HttpPost("Deactivate/{id}")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Deactivate(string id)
        {
            return SetActiveAsync(id, false);
        }

        //........................................................................................//

        [HttpPost("Activate/{id}")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> Activate(string id)
        {
            return SetActiveAsync(id, true);
        }

        //........................................................................................//

        private async Task<IActionResult> SetActiveAsync(string id, bool isActive)
        {
            var result = await _accountService.SetUserActiveAsync(id, isActive);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = isActive ? "User activated successfully." : "User deactivated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(" ", result.Errors);
            }

            return RedirectToAction(nameof(Index));
        }

        //........................................................................................//

        private static void PopulateRoleOptions(AdminUserCreateViewModel vm)
        {
            vm.RoleOptions = BuildRoleOptions(vm.Role);
        }

        //........................................................................................//

        private static void PopulateRoleOptions(AdminUserEditViewModel vm)
        {
            vm.RoleOptions = BuildRoleOptions(vm.Role);
        }

        //........................................................................................//

        private static List<SelectListItem> BuildRoleOptions(string selectedRole)
        {
            return ApplicationRoles.All
                .Select(role => new SelectListItem(role, role, role == selectedRole))
                .ToList();
        }

        //........................................................................................//

        private void AddErrors(AccountResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        //........................................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
