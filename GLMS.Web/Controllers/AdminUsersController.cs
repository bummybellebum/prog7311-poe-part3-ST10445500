using GLMS.Web.ApiClients;
using GLMS.Web.ApiModels;
using GLMS.Web.Security;
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
        private readonly IAuthApiClient _authApiClient;

        public AdminUsersController(IAuthApiClient authApiClient)
        {
            _authApiClient = authApiClient;
        }

        //........................................................................................//

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var result = await _authApiClient.GetUsersAsync();
            if (!result.IsSuccess)
            {
                ViewData["ErrorMessage"] = result.ErrorMessage;
                return View(new List<AdminUserListItemViewModel>());
            }

            return View(result.Data!
                .Select(ToListViewModel)
                .ToList());
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

            var result = await _authApiClient.CreateUserAsync(new CreateUserDto
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                Role = vm.Role,
                TemporaryPassword = vm.TemporaryPassword,
                ConfirmTemporaryPassword = vm.ConfirmTemporaryPassword,
                IsActive = vm.IsActive
            });

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction(nameof(Index));
            }

            AddError(result);
            PopulateRoleOptions(vm);
            return View(vm);
        }

        //........................................................................................//

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(string id)
        {
            var result = await _authApiClient.GetUserAsync(id);
            if (!result.IsSuccess || result.Data == null)
            {
                return NotFound();
            }

            var vm = ToEditViewModel(result.Data);
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

            var result = await _authApiClient.UpdateUserAsync(new UpdateUserDto
            {
                UserId = vm.UserId,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                Role = vm.Role,
                IsActive = vm.IsActive
            });

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            AddError(result);
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

            var result = await _authApiClient.ResetPasswordAsync(new ResetUserPasswordDto
            {
                UserId = vm.UserId,
                TemporaryPassword = vm.TemporaryPassword,
                ConfirmTemporaryPassword = vm.ConfirmTemporaryPassword
            });

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Temporary password set successfully.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            TempData["ErrorMessage"] = result.ErrorMessage;
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
            var result = await _authApiClient.SetUserActiveAsync(id, isActive);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = isActive ? "User activated successfully." : "User deactivated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
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

        private static AdminUserListItemViewModel ToListViewModel(UserListDto user)
        {
            return new AdminUserListItemViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        //........................................................................................//

        private static AdminUserEditViewModel ToEditViewModel(UserDetailDto user)
        {
            return new AdminUserEditViewModel
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            };
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
