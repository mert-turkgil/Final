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
    private readonly ICompanyRepository _companyRepository;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IWebHostEnvironment _hostingEnvironment; 
    private readonly IShopUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger,

    UserManager<User> userManager, 
    SignInManager<User> signInManager,
    RoleManager<ApplicationRole> roleManager,
    IShopUnitOfWork unitOfWork,
    ICompanyRepository companyRepository,
    IWebHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
        _roleManager = roleManager;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _companyRepository = companyRepository;
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
            
            // Retrieve companies with their topics, tools, and tool topics.
            // IMPORTANT: Call GetAllWithToolsAndTopicsAsync() (not GetAllTopicsAsync)
            var companies = await _companyRepository.GetAllWithToolsAndTopicsAsync(); 

            var companyDtos = companies.Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                BaseTopic = c.BaseTopic,
                // Optionally, if you want to support company-level topics:
               CompanyTopics = c.Topics?.Select(topic => new TopicDto
                {
                    Id = topic.Id,
                    BaseTopic = topic.BaseTopic,
                    TopicTemplate = topic.TopicTemplate,
                    HowMany = topic.HowMany,
                    DataType = topic.DataType.ToString(),
                    purpose = topic.TopicPurpose
                }).ToList() ?? new List<TopicDto>(),

                Tools = c.Tools.Select(t => new ToolDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    // Map topics including their purpose.
                    Topics = t.Topics.Select(topic => new TopicDto
                    {
                        Id = topic.Id,
                        BaseTopic = topic.BaseTopic,
                        TopicTemplate = topic.TopicTemplate,
                        HowMany = topic.HowMany,
                        DataType = topic.DataType.ToString(),
                        purpose = topic.TopicPurpose
                    }).ToList(),
                    // Optionally, if you want to show a tool’s overall purpose:
                    purpose = t.Topics.FirstOrDefault()?.TopicPurpose ?? TopicPurpose.Subscription
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
    #region Role Management Endpoints

    // GET: Display the role creation form.
    [HttpGet]
    public async Task<IActionResult> RoleCreate()
    {
        var model = new RoleCreateViewModel();
        await PopulateRoleCreateViewModelAsync(model);
        return View(model);
    }

    // POST: Process role creation.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoleCreate(RoleCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateRoleCreateViewModelAsync(model);
            return View(model);
        }

        // Create the new role using your custom ApplicationRole.
        var role = new ApplicationRole(model.RoleName);
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            await PopulateRoleCreateViewModelAsync(model);
            return View(model);
        }

        // Reload the role and cast it.
        var reloadedRole = await _roleManager.FindByIdAsync(role.Id);
        if (reloadedRole == null)
        {
            ModelState.AddModelError(string.Empty, "Role could not be reloaded after creation.");
            await PopulateRoleCreateViewModelAsync(model);
            return View(model);
        }
        var appRole = (ApplicationRole)reloadedRole;
        var roleId = appRole.Id;

        // Bind the role with the selected company.
        var company = await _unitOfWork.CompanyRepository.GetByIdAsync(model.CompanyId);
        if (company == null)
        {
            _logger.LogError("Company with ID {CompanyId} not found while binding role {RoleName}. Deleting the created role.", model.CompanyId, model.RoleName);
            await _roleManager.DeleteAsync(appRole);
            ModelState.AddModelError(string.Empty, "The selected company was not found. Role creation aborted.");
            await PopulateRoleCreateViewModelAsync(model);
            return View(model);
        }
        company.CompanyRoles.Add(new CompanyRole { CompanyId = company.Id, RoleId = roleId });
        await _unitOfWork.CompanyRepository.UpdateAsync(company);
        await _unitOfWork.SaveChangesAsync();

        // Bind selected users to the role.
        if (model.SelectedUserIds != null)
        {
            foreach (var userId in model.SelectedUserIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                    await _userManager.AddToRoleAsync(user, model.RoleName);
            }
        }

        TempData["SuccessMessage"] = "Role created successfully!";
        return RedirectToAction("Account");
    }

    // GET: Display the role editing form.
    [HttpGet]
    public async Task<IActionResult> RoleEdit(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            TempData["ErrorMessage"] = "Role not found.";
            return RedirectToAction("Account");
        }

        // Find the company bound to this role.
        var companies = await _unitOfWork.CompanyRepository.GetAllAsync();
        var boundCompany = companies.FirstOrDefault(c => c.CompanyRoles.Any(cr => cr.RoleId == role.Id));
        Guid companyId = boundCompany?.Id ?? Guid.Empty;

        // Retrieve users currently in the role.
        if (string.IsNullOrEmpty(role.Name))
        {
            TempData["ErrorMessage"] = "Role name is invalid.";
            return RedirectToAction("Account");
        }
        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
        var allUsers = await _userManager.Users.ToListAsync();

        var model = new RoleEditViewModel
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RoleEdit(RoleEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateRoleEditViewModelAsync(model);
            return View(model);
        }

        var role = await _roleManager.FindByIdAsync(model.Id);
        if (role == null)
        {
            TempData["ErrorMessage"] = "Role not found.";
            return RedirectToAction("Account");
        }
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
        if (role.Name.Equals("Root", StringComparison.OrdinalIgnoreCase) &&
            !(currentUser.Name.Equals("Root", StringComparison.OrdinalIgnoreCase) &&
                currentUser.LastName.Equals("Türkgil", StringComparison.OrdinalIgnoreCase)))
        {
            TempData["ErrorMessage"] = "Only the Root user can edit the Root role.";
            return RedirectToAction("Account");
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
                    ModelState.AddModelError(string.Empty, error.Description);
                await PopulateRoleEditViewModelAsync(model);
                return View(model);
            }
        }

        // Update company binding.
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

        // Update user assignments.
        var roleName = role.Name;
        var currentUsersInRole = await _userManager.GetUsersInRoleAsync(roleName);
        foreach (var user in currentUsersInRole)
            await _userManager.RemoveFromRoleAsync(user, roleName);

        if (model.SelectedUserIds != null && model.SelectedUserIds.Any())
        {
            foreach (var userId in model.SelectedUserIds)
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                    await _userManager.AddToRoleAsync(user, roleName);
            }
        }

        TempData["SuccessMessage"] = "Role updated successfully!";
        return RedirectToAction("Account");
    }

    // POST: Delete a role.
    [HttpPost]
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

        // Remove the role from all users.
        var roleName = role.Name;
        var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
        foreach (var user in usersInRole)
            await _userManager.RemoveFromRoleAsync(user, roleName);

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

        var deleteResult = await _roleManager.DeleteAsync(role);
        if (!deleteResult.Succeeded)
            TempData["ErrorMessage"] = "Error occurred while deleting the role.";
        else
            TempData["SuccessMessage"] = "Role deleted successfully!";

        return RedirectToAction("Account");
    }

    #region Helper Methods

    private async Task PopulateRoleCreateViewModelAsync(RoleCreateViewModel model)
    {
        var companies = await _unitOfWork.CompanyRepository.GetAllAsync();
        model.Companies = companies.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        }).ToList();

        var users = await _userManager.Users.ToListAsync();
        model.Users = users.Select(u => new SelectListItem
        {
            Value = u.Id,
            Text = u.UserName
        }).ToList();
    }

    private async Task PopulateRoleEditViewModelAsync(RoleEditViewModel model)
    {
        var companies = await _unitOfWork.CompanyRepository.GetAllAsync();
        model.Companies = companies.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        }).ToList();

        var users = await _userManager.Users.ToListAsync();
        model.Users = users.Select(u => new SelectListItem
        {
            Value = u.Id,
            Text = u.UserName
        }).ToList();
    }

    #endregion
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

        // 1. Create the Company entity.
        var company = new Company
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            BaseTopic = model.BaseTopic
        };

        // Process Company Topic Templates.
        if (model.CompanyTopicTemplates != null)
        {
            foreach (var ct in model.CompanyTopicTemplates)
            {
                await GenerateTopicsAsync(
                    company.BaseTopic,
                    ct.Template,  // Ensure the template uses "{seq}"
                    ct.HowMany,
                    ct.DataType,
                    ct.TopicPurpose,
                    mqttToolId: null,
                    companyId: company.Id);
            }
        }

        // Process Tools.
        if (model.Tools != null)
        {
            foreach (var toolModel in model.Tools)
            {
                var tool = new MqttTool
                {
                    Id = Guid.NewGuid(),
                    Name = toolModel.ToolName,
                    ToolBaseTopic = toolModel.ToolBaseTopic,
                    Description = toolModel.Description,
                    CompanyId = company.Id,
                    Company = company
                };

                // Process uploaded image file if provided.
                if (toolModel.ImageFile != null && toolModel.ImageFile.Length > 0)
                {
                    // Example: save the file to wwwroot/uploads and assign the URL.
                    var fileName = Path.GetFileName(toolModel.ImageFile.FileName);
                    var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await toolModel.ImageFile.CopyToAsync(stream);
                    }
                    tool.ImageUrl = "/uploads/" + fileName;
                }
                else
                {
                    // Optionally use any existing URL from a text field fallback.
                    tool.ImageUrl = toolModel.ImageUrl;
                }

                if (toolModel.TopicTemplates != null)
                {
                    foreach (var tt in toolModel.TopicTemplates)
                    {
                        await GenerateTopicsAsync(
                            company.BaseTopic,
                            tt.Template,  // Ensure template includes "{seq}"
                            tt.HowMany,
                            tt.DataType,
                            tt.TopicPurpose,
                            mqttToolId: tool.Id,
                            companyId: company.Id,
                            tool: tool);
                    }
                }
                company.Tools.Add(tool);
                await _unitOfWork.MqttToolRepository.AddAsync(tool);
            }
        }

        // Save the company (and its related topics and tools) to the database.
        await _unitOfWork.CompanyRepository.AddAsync(company);
        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "Company created successfully!";
        return RedirectToAction("Account");
    }

#region Helper Method
// This helper generates topics from a given template by replacing the "{seq}" placeholder
// with a sequence number (from 1 to count). It is used for both company-level and tool-specific topics.
private async Task GenerateTopicsAsync(
    string baseTopic,
    string template,
    int count,
    TopicDataType dataType,
    TopicPurpose topicPurpose,
    Guid? companyId = null,
    Guid? mqttToolId = null,
    MqttTool? tool = null)
{
    for (int i = 1; i <= count; i++)
    {
        // Replace the "{seq}" placeholder with the sequence number.
        var finalTemplate = template.Replace("{seq}", i.ToString());
        var topic = new MqttTopic
        {
            Id = Guid.NewGuid(),
            BaseTopic = baseTopic,
            TopicTemplate = finalTemplate,
            HowMany = 1, // Each generated topic represents a single entry.
            DataType = dataType,
            TopicPurpose = topicPurpose,
            MqttToolId = mqttToolId,
            CompanyId = companyId ?? throw new ArgumentNullException(nameof(companyId))
        };

        if (tool != null)
        {
            topic.MqttTool = tool;
            tool.Topics.Add(topic);
        }
        await _unitOfWork.MqttTopicRepository.AddAsync(topic);
    }
}
#endregion

#region EditCompany

// GET: Render the edit view for a company.
[HttpGet]
[Authorize(Roles = "Admin,Root")]
public async Task<IActionResult> EditCompany(Guid id)
{
    // Load the company with its tools and their topics.
    var company = await _unitOfWork.CompanyRepository.GetByIdWithToolsAsync(id);
    if (company == null)
    {
        return NotFound();
    }

    // Retrieve company-level topics (topics with no tool assigned).
    var companyTopics = await _unitOfWork.MqttTopicRepository.GetByCompanyIdAndNoToolAsync(company.Id);

    // Map the company to the edit view model.
    var model = new CompanyEditViewModel
    {
        Id = company.Id,
        Name = company.Name,
        BaseTopic = company.BaseTopic,
        CompanyTopicTemplates = companyTopics.Select(topic => new EditCompanyTopicTemplateViewModel
        {
            Template = topic.TopicTemplate,
            HowMany = topic.HowMany,
            DataType = topic.DataType,
            TopicPurpose = topic.TopicPurpose
        }).ToList(),
        Tools = company.Tools.Select(t => new ToolEditViewModel
        {
            Id = t.Id,
            ToolName = t.Name,
            ToolBaseTopic = t.ToolBaseTopic,
            Description = t.Description,
            ImageUrl = t.ImageUrl,
            TopicTemplates = t.Topics.Select(tt => new EditToolTopicTemplateViewModel
            {
                Template = tt.TopicTemplate,
                HowMany = tt.HowMany,
                DataType = tt.DataType,
                TopicPurpose = tt.TopicPurpose
            }).ToList()
        }).ToList()
    };

    return View(model);
}

// POST: Update an existing company along with its tools and topics.
[HttpPost]
[Authorize(Roles = "Admin,Root")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditCompany(CompanyEditViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    // Load the existing company (with its tools).
    var company = await _unitOfWork.CompanyRepository.GetByIdWithToolsAsync(model.Id);
    if (company == null)
    {
        return NotFound();
    }

    // Update basic company fields.
    company.Name = model.Name;
    company.BaseTopic = model.BaseTopic;

    // --- Update Company-Level Topics ---
    // Delete existing company-level topics.
    var existingCompanyTopics = await _unitOfWork.MqttTopicRepository.GetByCompanyIdAndNoToolAsync(company.Id);
    foreach (var topic in existingCompanyTopics)
    {
        await _unitOfWork.MqttTopicRepository.DeleteAsync(topic);
    }
    // Recreate company-level topics from the view model.
    if (model.CompanyTopicTemplates != null)
    {
        foreach (var ct in model.CompanyTopicTemplates)
        {
            await GenerateTopicsAsync(
                company.BaseTopic,
                ct.Template,
                ct.HowMany,
                ct.DataType,
                ct.TopicPurpose,
                mqttToolId: null,
                companyId: company.Id);
        }
    }

    // --- Update Tools and Their Topics ---
    // For simplicity, remove all existing tools (and their topics) and rebuild from the view model.
    foreach (var tool in company.Tools.ToList())
    {
        if (tool.Topics != null)
        {
            foreach (var topic in tool.Topics.ToList())
            {
                await _unitOfWork.MqttTopicRepository.DeleteAsync(topic);
            }
        }
        await _unitOfWork.MqttToolRepository.DeleteAsync(tool);
    }
    company.Tools.Clear();

    if (model.Tools != null)
    {
        foreach (var toolModel in model.Tools)
        {
            var tool = new MqttTool
            {
                // Reuse the existing Id if available; otherwise, assign a new one.
                Id = toolModel.Id != Guid.Empty ? toolModel.Id : Guid.NewGuid(),
                Name = toolModel.ToolName,
                ToolBaseTopic = toolModel.ToolBaseTopic,
                Description = toolModel.Description,
                CompanyId = company.Id
            };

            // Process uploaded image file if provided.
            if (toolModel.ImageFile != null && toolModel.ImageFile.Length > 0)
            {
                var fileName = Path.GetFileName(toolModel.ImageFile.FileName);
                var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await toolModel.ImageFile.CopyToAsync(stream);
                }
                tool.ImageUrl = "/uploads/" + fileName;
            }
            else
            {
                // Preserve the existing image URL if no new file is uploaded.
                tool.ImageUrl = toolModel.ImageUrl;
            }

            // Process topic templates for the tool.
            if (toolModel.TopicTemplates != null)
            {
                foreach (var tt in toolModel.TopicTemplates)
                {
                    await GenerateTopicsAsync(
                        company.BaseTopic,
                        tt.Template,
                        tt.HowMany,
                        tt.DataType,
                        tt.TopicPurpose,
                        mqttToolId: tool.Id,
                        companyId: company.Id,
                        tool: tool);
                }
            }
            company.Tools.Add(tool);
            await _unitOfWork.MqttToolRepository.AddAsync(tool);
        }
    }

    await _unitOfWork.CompanyRepository.UpdateAsync(company);
    await _unitOfWork.SaveChangesAsync();

    TempData["SuccessMessage"] = "Company updated successfully!";
    return RedirectToAction("Account", "Home");
}
#endregion
 
    [HttpPost]
    [Authorize(Roles = "Admin,Root")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCompany(Guid id)
    {
        // Retrieve the company with its related tools and topics.
        var company = await _unitOfWork.CompanyRepository.GetByIdWithToolsAsync(id);
        if (company == null)
        {
            return NotFound();
        }

        // Iterate over each tool in the company.
        foreach (var tool in company.Tools.ToList())
        {
            // If an image URL exists, delete the physical file.
            if (!string.IsNullOrEmpty(tool.ImageUrl))
            {
                // Remove any leading '/' and combine with the wwwroot path.
                var filePath = Path.Combine(_hostingEnvironment.WebRootPath, tool.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            
            // Delete topics for the tool.
            if (tool.Topics != null)
            {
                foreach (var topic in tool.Topics.ToList())
                {
                    await _unitOfWork.MqttTopicRepository.DeleteAsync(topic);
                }
            }
            // Delete the tool.
            await _unitOfWork.MqttToolRepository.DeleteAsync(tool);
        }

        // Finally, delete the company.
        await _unitOfWork.CompanyRepository.DeleteAsync(company);
        await _unitOfWork.SaveChangesAsync();

        TempData["SuccessMessage"] = "Company deleted successfully!";
        return RedirectToAction("Account", "Home");
    }
    #endregion
}
