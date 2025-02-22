using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Final.Models;
using Castle.Core.Smtp;
using Microsoft.AspNetCore.Identity;
using Final.Identity;
using Microsoft.AspNetCore.Localization;
using Final.Data.Abstract;
using Microsoft.AspNetCore.Authorization;
using Final.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

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
                ViewBag.AlreadyLoggedIn = true;
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
        
        // Retrieve companies (with related tools and topics).
        var companies = await _unitOfWork.CompanyRepository.GetAllAsync();

        // Map companies into CompanyDto, including RoleIds from the CompanyRoles join.
        var companyDtos = companies.Select(c => new CompanyDto
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
            }).ToList(),
            RoleIds = c.CompanyRoles.Select(cr => cr.RoleId).ToList()
        }).ToList();

        // Retrieve all available roles.
        var roles = _roleManager.Roles.ToList();

        // Build the user view model.
        var userViewModel = new UserViewModel
        {
            Id = currentUser.Id,
            UserName = currentUser.UserName,
            FirstName = currentUser.Name,
            LastName = currentUser.LastName,
            Companies = companyDtos,
            Roles = roles
        };

        return View(userViewModel);
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

    [Authorize(Roles = "Admin,Root")]
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
            // Determine lockout status: user is locked if lockout is enabled and lockout end is in the future.
            Lockout = user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow,
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
        
        // Update Lockout settings based on the model
        user.LockoutEnabled = model.Lockout;
        if (model.Lockout)
        {
            // Lock the user indefinitely (or set a specific future date)
            user.LockoutEnd = DateTimeOffset.MaxValue;
        }
        else
        {
            user.LockoutEnd = null;
        }

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

    // GET: Display the user creation form
    [HttpGet]
    [Authorize(Roles = "Admin,Root")]
    public IActionResult UserCreate()
    {
        var model = new Final.Models.UserCreateModel
        {
            // Populate available roles from RoleManager
            AllRoles = _roleManager.Roles.Select(r => r.Name).ToList(),
            // Set defaults as needed (e.g. EmailConfirmed = false)
            EmailConfirmed = false,
            LockoutEnabled = false
        };

        return View(model);
    }

    // POST: Create a new user
    [HttpPost]
    [Authorize(Roles = "Admin,Root")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserCreate(Final.Models.UserCreateModel model)
    {
        if (!ModelState.IsValid)
        {
            // Repopulate roles in case of error
            model.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // Create a new user instance (assuming your user class is named "User" and lives in Final.Identity)
        var user = new Final.Identity.User
        {
            Name = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            UserName = model.UserName,
            EmailConfirmed = model.EmailConfirmed,
            CreatedDate = DateTime.UtcNow,
            LockoutEnabled = model.LockoutEnabled,
            // If lockout is enabled, use provided LockoutEnd or lock indefinitely
            LockoutEnd = model.LockoutEnabled 
                            ? (model.LockoutEnd.HasValue ? model.LockoutEnd : DateTimeOffset.MaxValue) 
                            : null
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            // Repopulate roles list
            model.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // Assign roles if any have been selected
        if (model.SelectedRoles != null && model.SelectedRoles.Any())
        {
            var roleResult = await _userManager.AddToRolesAsync(user, model.SelectedRoles);
            if (!roleResult.Succeeded)
            {
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                // Optionally, handle cleanup (e.g. remove the user if roles assignment fails)
                model.AllRoles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(model);
            }
        }

        TempData["SuccessMessage"] = "User created successfully!";
        return RedirectToAction("Account"); // Adjust redirection as needed
    }

    #endregion

    #region Roles
    // GET: Display the role creation form.
    [HttpGet]
    [Authorize(Roles = "Admin,Root")]
    public async Task<IActionResult> RoleCreate()
    {
        var companies = await _unitOfWork.CompanyRepository.GetAllAsync();
        var users = await _userManager.Users.ToListAsync();

        var model = new Final.Models.RoleCreateViewModel
        {
            Companies = companies.Select(c => new SelectListItem 
            { 
                Value = c.Id.ToString(), 
                Text = c.Name 
            }).ToList(),
            Users = users.Select(u => new SelectListItem 
            { 
                Value = u.Id, 
                Text = u.UserName 
            }).ToList()
        };
        return View(model);
    }

    // POST: Process the role creation.
    [HttpPost]
    [Authorize(Roles = "Admin,Root")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoleCreate(Final.Models.RoleCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Repopulate the lists and return the view.
            var companies = await _unitOfWork.CompanyRepository.GetAllAsync();
            var users = await _userManager.Users.ToListAsync();

            model.Companies = companies.Select(c => new SelectListItem 
            { 
                Value = c.Id.ToString(), 
                Text = c.Name 
            }).ToList();
            model.Users = users.Select(u => new SelectListItem 
            { 
                Value = u.Id, 
                Text = u.UserName 
            }).ToList();
            return View(model);
        }

        // Create a new role.
        var role = new IdentityRole(model.RoleName);
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            // Repopulate and return.
            var companies = await _unitOfWork.CompanyRepository.GetAllAsync();
            var users = await _userManager.Users.ToListAsync();
            model.Companies = companies.Select(c => new SelectListItem 
            { 
                Value = c.Id.ToString(), 
                Text = c.Name 
            }).ToList();
            model.Users = users.Select(u => new SelectListItem 
            { 
                Value = u.Id, 
                Text = u.UserName 
            }).ToList();
            return View(model);
        }

        // Bind the role with a company.
        var company = await _unitOfWork.CompanyRepository.GetByIdAsync(model.CompanyId);
        if (company != null)
        {
            company.CompanyRoles.Add(new CompanyRole 
            { 
                CompanyId = company.Id, 
                RoleId = role.Id 
            });
            await _unitOfWork.CompanyRepository.UpdateAsync(company);
            await _unitOfWork.SaveChangesAsync();
        }

        // Bind selected users to the role.
        if (model.SelectedUserIds != null)
        {
            foreach (var userId in model.SelectedUserIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _userManager.AddToRoleAsync(user, model.RoleName);
                }
            }
        }

        TempData["SuccessMessage"] = "Role created successfully!";
        return RedirectToAction("Account");
    }

    // GET: Display the role editing form.
    [HttpGet]
    [Authorize(Roles = "Admin,Root")]
    public async Task<IActionResult> RoleEdit(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            TempData["ErrorMessage"] = "Role not found.";
            return RedirectToAction("Account");
        }

        // Determine which company is bound to this role.
        var companies = await _unitOfWork.CompanyRepository.GetAllAsync();
        Guid companyId = Guid.Empty;
        foreach (var comp in companies)
        {
            var compRole = comp.CompanyRoles.FirstOrDefault(cr => cr.RoleId == role.Id);
            if (compRole != null)
            {
                companyId = comp.Id;
                break;
            }
        }

        // Retrieve users currently in the role.
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        var allUsers = await _userManager.Users.ToListAsync();

        var model = new Final.Models.RoleEditViewModel
        {
            Id = role.Id,
            RoleName = role.Name,
            CompanyId = companyId,
            Companies = companies.Select(c => new SelectListItem 
            { 
                Value = c.Id.ToString(), 
                Text = c.Name 
            }).ToList(),
            Users = allUsers.Select(u => new SelectListItem 
            { 
                Value = u.Id, 
                Text = u.UserName 
            }).ToList(),
            SelectedUserIds = usersInRole.Select(u => u.Id).ToList()
        };

        return View(model);
    }

    // POST: Process the role edit.
    [HttpPost]
    [Authorize(Roles = "Admin,Root")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoleEdit(Final.Models.RoleEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var companies = await _unitOfWork.CompanyRepository.GetAllAsync();
            var users = await _userManager.Users.ToListAsync();
            model.Companies = companies.Select(c => new SelectListItem 
            { 
                Value = c.Id.ToString(), 
                Text = c.Name 
            }).ToList();
            model.Users = users.Select(u => new SelectListItem 
            { 
                Value = u.Id, 
                Text = u.UserName 
            }).ToList();
            return View(model);
        }

        var role = await _roleManager.FindByIdAsync(model.Id);
        if (role == null)
        {
            TempData["ErrorMessage"] = "Role not found.";
            return RedirectToAction("Account");
        }

        // Ensure role name is valid.
        if (string.IsNullOrWhiteSpace(role.Name))
        {
            TempData["ErrorMessage"] = "Invalid role name.";
            return RedirectToAction("Account");
        }

        // Get the current user.
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            TempData["ErrorMessage"] = "Current user not found.";
            return RedirectToAction("Account");
        }

        // Restrict editing of the Root role.
        if (role.Name.Equals("Root", StringComparison.OrdinalIgnoreCase))
        {
            // Only a user with the Root identity can edit the Root role.
            if (!(currentUser.Name.Equals("Root", StringComparison.OrdinalIgnoreCase) &&
                currentUser.LastName.Equals("Türkgil", StringComparison.OrdinalIgnoreCase)))
            {
                TempData["ErrorMessage"] = "Only the Root user can edit the Root role.";
                return RedirectToAction("Account");
            }
        }

        // Restrict editing of the Admin role.
        if (role.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            var isCurrentAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var isCurrentRoot = currentUser.Name.Equals("Root", StringComparison.OrdinalIgnoreCase) &&
                                currentUser.LastName.Equals("Türkgil", StringComparison.OrdinalIgnoreCase);
            if (!isCurrentAdmin && !isCurrentRoot)
            {
                TempData["ErrorMessage"] = "Only Admin or Root users can edit the Admin role.";
                return RedirectToAction("Account");
            }
        }

        // Update role name if changed.
        if (!role.Name.Equals(model.RoleName, StringComparison.OrdinalIgnoreCase))
        {
            role.Name = model.RoleName;
            var updateResult = await _roleManager.UpdateAsync(role);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                var companies = await _unitOfWork.CompanyRepository.GetAllAsync();
                var users = await _userManager.Users.ToListAsync();
                model.Companies = companies.Select(c => new SelectListItem 
                { 
                    Value = c.Id.ToString(), 
                    Text = c.Name 
                }).ToList();
                model.Users = users.Select(u => new SelectListItem 
                { 
                    Value = u.Id, 
                    Text = u.UserName 
                }).ToList();
                return View(model);
            }
        }

        // Update company binding: Remove old binding and add new if necessary.
        var allCompanies = await _unitOfWork.CompanyRepository.GetAllAsync();
        foreach (var comp in allCompanies)
        {
            var compRole = comp.CompanyRoles.FirstOrDefault(cr => cr.RoleId == role.Id);
            if (compRole != null && comp.Id != model.CompanyId)
            {
                comp.CompanyRoles.Remove(compRole);
                await _unitOfWork.CompanyRepository.UpdateAsync(comp);
            }
        }
        var newCompany = await _unitOfWork.CompanyRepository.GetByIdAsync(model.CompanyId);
        if (newCompany != null && !newCompany.CompanyRoles.Any(cr => cr.RoleId == role.Id))
        {
            newCompany.CompanyRoles.Add(new CompanyRole { CompanyId = newCompany.Id, RoleId = role.Id });
            await _unitOfWork.CompanyRepository.UpdateAsync(newCompany);
        }
        await _unitOfWork.SaveChangesAsync();

        // Update user assignments:
        // Remove all current users from this role.
        var roleName = role.Name; // Already verified to be non-null.
        var currentUsers = await _userManager.GetUsersInRoleAsync(roleName);
        foreach (var user in currentUsers)
        {
            await _userManager.RemoveFromRoleAsync(user, roleName);
        }
        // Add selected users to the role, if any.
        if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
        {
            foreach (var userId in model.SelectedUserIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    await _userManager.AddToRoleAsync(user, roleName);
                }
            }
        }
        // If SelectedUserIds is null or empty, that's acceptable – the role will have no users.

        TempData["SuccessMessage"] = "Role updated successfully!";
        return RedirectToAction("Account");
    }

    // POST: Delete a role.
    [HttpPost]
    [Authorize(Roles = "Admin,Root")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoleDelete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            TempData["ErrorMessage"] = "Role not found.";
            return RedirectToAction("Account");
        }

        // Prevent deletion of the Root role.
        if (string.IsNullOrWhiteSpace(role.Name) || role.Name.Equals("Root", StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "The Root role cannot be deleted.";
            return RedirectToAction("Account");
        }

        // Guard against null by using role.Name after checking.
        var roleName = role.Name!; // Now safe to assume non-null.

        // Remove the role from all users.
        var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
        foreach (var user in usersInRole)
        {
            await _userManager.RemoveFromRoleAsync(user, roleName);
        }

        // Remove any company binding.
        var companies = await _unitOfWork.CompanyRepository.GetAllAsync();
        foreach (var comp in companies)
        {
            var compRole = comp.CompanyRoles.FirstOrDefault(cr => cr.RoleId == role.Id);
            if (compRole != null)
            {
                comp.CompanyRoles.Remove(compRole);
                await _unitOfWork.CompanyRepository.UpdateAsync(comp);
            }
        }
        await _unitOfWork.SaveChangesAsync();

        // Delete the role.
        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = "Error occurred while deleting the role.";
        }
        else
        {
            TempData["SuccessMessage"] = "Role deleted successfully!";
        }
        return RedirectToAction("Account");
    }


    #endregion

    #region Company
    // GET: Render the company creation view.
    [HttpGet]
    [Authorize(Roles = "Admin,Root")]
    public IActionResult CreateCompany()
    {
        return View(new CompanyCreateViewModel());
    }

    // POST: Create a new company along with its MqttTool and MqttTopic.
    [HttpPost]
    [Authorize(Roles = "Admin,Root")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCompany(CompanyCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Create the Company entity.
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            BaseTopic = model.BaseTopic
        };

        // Create Company-level Subscription Topics.
        for (int i = 1; i <= model.HowMany; i++)
        {
            var subTopic = new MqttTopic
            {
                Id = Guid.NewGuid(),
                BaseTopic = company.BaseTopic,
                // Example pattern: {BaseTopic}/{TopicTemplate}/{TopicTemplate}/{TopicTemplate}{i}/{TopicTemplate}
                TopicTemplate = $"{model.TopicTemplate}/{model.TopicTemplate}/{model.TopicTemplate}{i}/{model.TopicTemplate}",
                HowMany = 1,
                DataType = model.DataType,
                MqttToolId = null  // Company-level topic.
            };
            await _unitOfWork.MqttTopicRepository.AddAsync(subTopic);
        }

        // Create Company-level Sending Topics (flexible, based on the sending topics submitted).
        if(model.SendingTopics != null)
        {
            foreach (var sending in model.SendingTopics)
            {
                for (int i = 1; i <= sending.HowMany; i++)
                {
                    var sendTopic = new MqttTopic
                    {
                        Id = Guid.NewGuid(),
                        BaseTopic = company.BaseTopic,
                        // Here you can decide on a pattern. For example:
                        TopicTemplate = $"{sending.TopicTemplate}{i}",
                        HowMany = 1,
                        DataType = sending.DataType,
                        MqttToolId = null  // Company-level sending topic.
                    };
                    await _unitOfWork.MqttTopicRepository.AddAsync(sendTopic);
                }
            }
        }

        // Process Tools (similar to previous logic; create MqttTool entities with their own topics).
        // For brevity, assuming one default tool is created as an example.
        var mqttTool = new MqttTool
        {
            Id = Guid.NewGuid(),
            Name = model.ToolName,
            ToolBaseTopic = model.ToolBaseTopic,
            Description = model.Description,
            ImageUrl = model.ImageUrl,
            CompanyId = company.Id,
            Company = company
        };

        var toolTopic = new MqttTopic
        {
            Id = Guid.NewGuid(),
            BaseTopic = company.BaseTopic,
            TopicTemplate = $"{model.TopicTemplate}1/status", // Modify as needed.
            HowMany = 1,
            DataType = model.DataType,
            MqttToolId = mqttTool.Id,
            MqttTool = mqttTool
        };
        mqttTool.Topics.Add(toolTopic);
        company.Tools.Add(mqttTool);

        await _unitOfWork.CompanyRepository.AddAsync(company);
        await _unitOfWork.MqttToolRepository.AddAsync(mqttTool);
        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "Company created successfully!";
        return RedirectToAction("Account");
    }



    #endregion
}
