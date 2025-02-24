using System.Globalization;
using System.Reflection;
using Final.Configuration;
using Final.Data;
using Final.Data.Abstract;
using Final.Data.Concrete;
using Final.Hubs;
using Final.Identity;
using Final.Services;
using Final.Services.Hosted;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

#region Database configuration
builder.Services.Configure<HttpsRedirectionOptions>(options =>
{
    options.HttpsPort = 5001; // or the port you use for HTTPS
});

builder.Services.Configure<MqttConfig>(builder.Configuration.GetSection("MqttConfig"));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MsSqlConnection")));

builder.Services.AddDbContext<ShopContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MsSqlConnection"));
    options.UseLazyLoadingProxies();
});
#endregion

#region Identity & Cookie Configuration
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    
    // Sign-in settings
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;

    // Password policy
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;

    // Lockout policy
    options.Lockout.MaxFailedAccessAttempts = 4;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(4);
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // Set the RoleClaimType directly here
    options.ClaimsIdentity.RoleClaimType = System.Security.Claims.ClaimTypes.Role;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins("http://localhost:5196")
                .AllowAnyHeader()
                .WithMethods("GET", "POST")
                .AllowCredentials();
        });
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Home/Index"; 
    options.LogoutPath = "/logout"; 
    options.AccessDeniedPath = "/accessdenied"; 
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie = new CookieBuilder
    {
        HttpOnly = true, 
        Name = ".FinalCop.Security.Cookie",
        SameSite = SameSiteMode.Lax, 
        SecurePolicy = CookieSecurePolicy.Always, 
        IsEssential = true 
    };
});
#endregion

#region Register injection
builder.Services.AddSession();
builder.Services.AddScoped<IShopUnitOfWork, ShopUnitOfWork>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IMqttToolRepository, MqttToolRepository>();
builder.Services.AddScoped<IMqttTopicRepository, MqttTopicRepository>();
builder.Services.AddSingleton<IMqttService, MqttService>();
builder.Services.AddSingleton<IMqttLogService, MqttLogService>();
// Register the hosted MQTT background service once
builder.Services.AddHostedService<MqttBackgroundService>();
#endregion

#region Services & SignalR
builder.Services.AddSignalR();
#endregion

#region Localization and MVC
// Localization
builder.Services.AddSingleton<LanguageService>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc()
    .AddSessionStateTempDataProvider()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        var assemblyInfo = typeof(SharedResource).GetTypeInfo().Assembly;

        if (assemblyInfo == null)
            throw new InvalidOperationException("Assembly information for SharedResource is missing.");

        var assemblyName = new AssemblyName(assemblyInfo.FullName ?? throw new InvalidOperationException("Assembly full name cannot be null."));
        
        if (string.IsNullOrEmpty(assemblyName.Name))
            throw new InvalidOperationException("Assembly name cannot be null or empty.");

        options.DataAnnotationLocalizerProvider = (type, factory) =>
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory), "Localizer factory cannot be null.");
            
            // Ensure location is not null
            var location = assemblyName.Name ?? throw new ArgumentNullException(nameof(assemblyName.Name), "Location cannot be null.");
            return factory.Create(nameof(SharedResource), location);
        };
    });


builder.Services.Configure<RequestLocalizationOptions>(options => {
    var supportedCultures = new List<CultureInfo> {
        new CultureInfo("en-US"),
        new CultureInfo("tr-TR")
    };
    options.DefaultRequestCulture = new RequestCulture("tr-TR", "tr-TR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
});
#endregion

// Now build the app once all registrations are complete
var app = builder.Build();

#region Localization Middleware
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);
#endregion

#region User Checking & Seeding
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var usersConfig = configuration.GetSection("Data:Users").GetChildren();
    if (usersConfig == null || !usersConfig.Any())
    {
        Console.WriteLine("No user configuration found.");
    }
    else
    {
        var rootUserSection = usersConfig.FirstOrDefault(user =>
            user.GetValue<string>("UserName")?.Equals("Root", StringComparison.OrdinalIgnoreCase) == true);
        var adminUserSection = usersConfig.FirstOrDefault(user =>
            user.GetValue<string>("UserName")?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true);

        bool seedRequired = false;

        if (rootUserSection != null)
        {
            var rootEmail = rootUserSection.GetValue<string>("email");
            if (string.IsNullOrEmpty(rootEmail))
                throw new ArgumentNullException(nameof(rootEmail), "Root email cannot be null or empty.");

            var rootUser = await userManager.FindByEmailAsync(rootEmail!);
            if (rootUser == null)
            {
                Console.WriteLine("Root user does not exist.");
                seedRequired = true;
            }
            else
            {
                // Check if the root user is in the required roles ("Root" and "Admin")
                bool hasRootRole = await userManager.IsInRoleAsync(rootUser, "Root");
                bool hasAdminRole = await userManager.IsInRoleAsync(rootUser, "Admin");

                if (!hasRootRole || !hasAdminRole)
                {
                    Console.WriteLine("Root user missing required roles. Seeding roles for Root user...");

                    if (!hasRootRole)
                    {
                        var addRootResult = await userManager.AddToRoleAsync(rootUser, "Root");
                        if (addRootResult.Succeeded)
                        {
                            Console.WriteLine("Added 'Root' role to Root user.");
                        }
                        else
                        {
                            foreach (var error in addRootResult.Errors)
                            {
                                Console.WriteLine($"Error adding 'Root' role to Root user: {error.Description}");
                            }
                        }
                    }

                    if (!hasAdminRole)
                    {
                        var addAdminResult = await userManager.AddToRoleAsync(rootUser, "Admin");
                        if (addAdminResult.Succeeded)
                        {
                            Console.WriteLine("Added 'Admin' role to Root user.");
                        }
                        else
                        {
                            foreach (var error in addAdminResult.Errors)
                            {
                                Console.WriteLine($"Error adding 'Admin' role to Root user: {error.Description}");
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Root user already exists with required roles.");
                }
            }
        }
        else
        {
            Console.WriteLine("Root user configuration not found.");
        }

        if (adminUserSection != null)
        {
            var adminEmail = adminUserSection.GetValue<string>("email");
            if (string.IsNullOrEmpty(adminEmail))
                throw new ArgumentNullException(nameof(adminEmail), "Admin email cannot be null or empty.");

            var adminUser = await userManager.FindByEmailAsync(adminEmail!);
            if (adminUser == null)
            {
                Console.WriteLine("Admin user does not exist.");
                seedRequired = true;
            }
            else
            {
                Console.WriteLine("Admin user already exists.");
            }
        }
        else
        {
            Console.WriteLine("Admin user configuration not found.");
        }

        if (seedRequired)
        {
            Console.WriteLine("One or more critical users are missing. Seeding roles and users...");
            await SeedIdentity.Seed(userManager, roleManager, configuration);
        }
        else
        {
            Console.WriteLine("Both critical users exist. Skipping seed process.");
        }
    }
}
#endregion

#region Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors();
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.UseSession();
#endregion

app.Run();
