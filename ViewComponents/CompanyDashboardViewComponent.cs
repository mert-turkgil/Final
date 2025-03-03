using System.Linq;
using System.Threading.Tasks;
using Final.Data.Abstract;
using Final.Entity;
using Final.Identity;
using Final.Models.ViewComponents;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Final.ViewComponents
{
    public class CompanyDashboardViewComponent : ViewComponent
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly UserManager<User> _userManager;

        public CompanyDashboardViewComponent(ICompanyRepository companyRepository, UserManager<User> userManager)
        {
            _companyRepository = companyRepository;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Get the current user and his/her roles.
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            var userRoles = currentUser != null 
                ? await _userManager.GetRolesAsync(currentUser) 
                : new System.Collections.Generic.List<string>();

            // Retrieve companies with their tools and topics.
            var companies = await _companyRepository.GetAllWithToolsAndTopicsAsync();
            
            // Map companies to view models.
            var companyViewModels = companies.Select(c => new CompanyViewModel
            {
                Id = c.Id,
                Name = c.Name,
                BaseTopic = c.BaseTopic,
                CompanyTopics = c.Topics
                    .Where(t => t.TopicPurpose == TopicPurpose.Sending)
                    .Select(t => new TopicViewModel
                    {
                        Id = t.Id,
                        TopicTemplate = t.TopicTemplate,
                        HowMany = t.HowMany,
                        DataType = t.DataType.ToString(),
                        Purpose = t.TopicPurpose
                    }).ToList(),
                Tools = c.Tools.Select(t => new ToolViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    ToolBaseTopic = t.ToolBaseTopic,
                    Description = t.Description,
                    ImageUrl = t.ImageUrl,
                    Topics = t.Topics
                        .Where(tt => tt.TopicPurpose == TopicPurpose.Sending)
                        .Select(tt => new TopicViewModel
                        {
                            Id = tt.Id,
                            TopicTemplate = tt.TopicTemplate,
                            HowMany = tt.HowMany,
                            DataType = tt.DataType.ToString(),
                            Purpose = tt.TopicPurpose
                        }).ToList()
                }).ToList(),
                RoleIds = c.CompanyRoles.Select(cr => cr.RoleId).ToList()
            })
            // Filter: if the user is Admin/Root, show all; otherwise, show only companies where at least one company role matches the user's roles.
            .Where(cv => userRoles.Contains("Admin") 
                         || userRoles.Contains("Root") 
                         || cv.RoleIds.Any(role => userRoles.Contains(role)))
            .ToList();

            var viewModel = new CompanyDashboardViewModel
            {
                Companies = companyViewModels
            };

            return View(viewModel);
        }
    }
}
