using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Final.Models;
using Castle.Core.Smtp;
using Microsoft.AspNetCore.Identity;
using Final.Identity;
using Microsoft.AspNetCore.Localization;
using Final.Data.Abstract;
using Microsoft.AspNetCore.Authorization;

namespace Final.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    private readonly IShopUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger,
    UserManager<User> userManager, 
    SignInManager<User> signInManager,
    RoleManager<IdentityRole> roleManager,
    IShopUnitOfWork unitOfWork)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }
        #region Login - Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Account");
            }
            return View(new LoginViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Account");
            }
            
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Please provide valid inputs.");
                return View(model);
            }
            
            // Find the user by username
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt: user not found.");
                return View(model);
            }
            
            if (string.IsNullOrEmpty(user.UserName))
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            // Check if the account is locked out
            if (await _userManager.IsLockedOutAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Your account is locked. Please try again later.");
                return View(model);
            }

            // Attempt to sign in the user with the provided password
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                return RedirectToAction("Account");
            }
            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "Login is not allowed.");
                return View(model);
            }
            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Your account is locked due to too many failed attempts.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your credentials.");
            return View(model);
        }
        #endregion
        #region Odalar ve Yönetim kısmı
        [HttpGet]
        public async Task<IActionResult> Account()
        {
            // Get the currently logged-in user.
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Index");
            }
            
            // Retrieve companies (with related tools and topics) from your repository.
            // Ensure your repository either uses eager loading or lazy loading as needed.
            var companies = await _unitOfWork.CompanyRepository.GetAllAsync();

            // Map the data into the UserViewModel.
            var viewModel = new UserViewModel
            {
                UserName = currentUser.UserName,
                FirstName = currentUser.Name,
                LastName = currentUser.LastName,
                Companies = companies.Select(c => new CompanyDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    BaseTopic = c.BaseTopic,
                    Tools = c.Tools.Select(t => new ToolDto
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        Topics = t.Topics.Select(topic => new TopicDto
                        {
                            Id = topic.Id,
                            BaseTopic = topic.BaseTopic,
                            TopicTemplate = topic.TopicTemplate,
                            HowMany = topic.HowMany,
                            DataType = topic.DataType.ToString()
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            return View(viewModel);
        }
        #endregion
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
        #region SetLanguage
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
            return Redirect(Request.Headers["Referer"].ToString());
        }

    #endregion
        #region User

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> UserDelete(string id)
    {
        // Find the user by ID
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Account");
        }

        // Check if the target user is the Root user.
        bool isTargetRoot = user.Name == "Root" && user.LastName == "Türkgil";
        if (isTargetRoot)
        {
            TempData["ErrorMessage"] = "You cannot delete the Root user.";
            return RedirectToAction("Account");
        }

        // Get the roles for the target user.
        var roles = await _userManager.GetRolesAsync(user);
        // If the target user is an admin, block deletion.
        if (roles.Contains("Admin"))
        {
            TempData["ErrorMessage"] = "You cannot delete an Admin user.";
            return RedirectToAction("Account");
        }

        // Proceed to delete the user.
        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "User deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Error occurred while deleting the user.";
        }

        return RedirectToAction("Account");
    }

    // Edit User Method (GET)
    [HttpGet("Home/UserEdit/{id}")]
    [Authorize(Roles = "Admin,Root")]
    public async Task<IActionResult> UserEdit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Account");
        }

        var currentUser = await _userManager.GetUserAsync(User!);
        var currentRoles = await _userManager.GetRolesAsync(currentUser!);

        // Only Admins/Root can edit Admins
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        var currentUserIsAdminOrRoot = currentRoles.Contains("Admin") || currentRoles.Contains("Root");

        if (isAdmin && !currentUserIsAdminOrRoot)
        {
            TempData["ErrorMessage"] = "You do not have permission to edit this user.";
            return RedirectToAction("Account");
        }

        var model = new UserEditModel
        {
            UserId = user.Id,
            CreatedDate = user.CreatedDate,
            FirstName = user.Name,
            LastName = user.LastName,
            Email = user.Email,
            UserName = user.UserName,
            EmailConfirmed = user.EmailConfirmed,
            SelectedRoles = (await _userManager.GetRolesAsync(user)).ToList(),
            AllRoles = _roleManager.Roles.Select(r => r.Name).ToList()
        };

        return View(model);
    }

    // Edit User Method (POST)
    [HttpPost("Home/UserEdit/{id}")]
    [Authorize(Roles = "Admin,Root")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserEdit(UserEditModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Account");
        }

        var currentUser = await _userManager.GetUserAsync(User!);
        var currentRoles = await _userManager.GetRolesAsync(currentUser!);

        // Only Root can edit Root
        var isTargetRoot = user.Name == "Root" && user.LastName == "Türkgil";
        var isCurrentRoot = currentUser!.Name == "Root" && currentUser.LastName == "Türkgil";

        if (isTargetRoot && !isCurrentRoot)
        {
            TempData["ErrorMessage"] = "Only the Root user can edit the Root user.";
            return RedirectToAction("Account");
        }

        // Only Admin/Root can edit Admin users
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        var currentUserIsAdminOrRoot = currentRoles.Contains("Admin") || currentRoles.Contains("Root");

        if (isAdmin && !currentUserIsAdminOrRoot)
        {
            TempData["ErrorMessage"] = "You do not have permission to edit this user.";
            return RedirectToAction("Account");
        }

        // Update User Information
        user.CreatedDate = model.CreatedDate;
        user.Name = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.UserName = model.UserName;
        user.EmailConfirmed = model.EmailConfirmed;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            model.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // Update Roles
        var currentRolesForUser = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRolesForUser);
        await _userManager.AddToRolesAsync(user, model.SelectedRoles ?? new List<string>());

        // If a new password is provided, update it using a reset token.
        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Password);
            if (!passwordResult.Succeeded)
            {
                foreach (var error in passwordResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                model.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(model);
            }
        }

        TempData["SuccessMessage"] = "User updated successfully!";
        return RedirectToAction("Account");
    }
        #endregion

}
