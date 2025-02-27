using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Final.Identity
{
    public static class SeedIdentity
    {
        public static async Task Seed(UserManager<User> userManager, RoleManager<ApplicationRole> roleManager, IConfiguration configuration)
        {
            // Seed roles from configuration
            var roles = configuration.GetSection("Data:Roles").Get<string[]>();
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new ApplicationRole(role));
                    }
                }
            }

            // Seed or update users from configuration
            var usersSection = configuration.GetSection("Data:Users");
            foreach (var section in usersSection.GetChildren())
            {
                #nullable disable
                var username = section.GetValue<string>("username");
                var password = section.GetValue<string>("password");
                var email = section.GetValue<string>("email");
                var roleConfig = section.GetValue<string>("role");
                var firstName = section.GetValue<string>("firstName");
                var telNumber = section.GetValue<string>("telNumber");
                var lastName = section.GetValue<string>("lastName");

                // Retrieve existing user or create a new one
                var user = await userManager.FindByNameAsync(username);
                if (user == null)
                {
                    user = new User
                    {
                        UserName = username,
                        Email = email,
                        Name = firstName,
                        EmailAddress = email,
                        LastName = lastName,
                        TelNumber = telNumber,
                        CreatedDate = DateTime.Now,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(user, password);
                    if (!createResult.Succeeded)
                    {
                        foreach (var error in createResult.Errors)
                        {
                            Console.WriteLine($"Error creating user '{username}': {error.Description}");
                        }
                        continue;
                    }
                    else
                    {
                        Console.WriteLine($"User '{username}' created successfully.");
                    }
                }
                else
                {
                    Console.WriteLine($"User '{username}' already exists. Updating roles if needed.");
                }

                // Always update roles for the user (even if they already existed)
                if (!string.IsNullOrWhiteSpace(roleConfig))
                {
                    var rolesToAssign = roleConfig
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(r => r.Trim());

                    foreach (var r in rolesToAssign)
                    {
                        if (!await userManager.IsInRoleAsync(user, r))
                        {
                            var addRoleResult = await userManager.AddToRoleAsync(user, r);
                            if (addRoleResult.Succeeded)
                            {
                                Console.WriteLine($"Added role '{r}' to user '{username}'.");
                            }
                            else
                            {
                                foreach (var error in addRoleResult.Errors)
                                {
                                    Console.WriteLine($"Error adding role '{r}' to user '{username}': {error.Description}");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
