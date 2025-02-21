using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Final.Data.Abstract;
using Final.Entity;           
using Final.Identity;
using Final.Models;             
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Final.ViewComponents
{
    public class RoleManagementViewComponent : ViewComponent
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IShopUnitOfWork _unitOfWork;

        public RoleManagementViewComponent(RoleManager<IdentityRole> roleManager, IShopUnitOfWork unitOfWork)
        {
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        // Renders the view component's UI with the list of roles and companies.
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var roles = _roleManager.Roles.ToList();
            var companies = await _unitOfWork.CompanyRepository.GetAllAsync();

            var companyDtos = companies.Select(c => new CompanyDto
            {
                Id = c.Id,
                Name = c.Name,
                BaseTopic = c.BaseTopic,
                RoleIds = c.CompanyRoles.Select(cr => cr.RoleId).ToList()
            }).ToList();

            var viewModel = new Final.Models.RoleManagementViewModel
            {
                Roles = roles,
                Companies = companyDtos
            };


            return View(viewModel);
        }
        // Creates a new role with the given name.
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                return new JsonResult(new { message = "Role name is required" });
            }
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            return result.Succeeded
                ? new JsonResult(new { message = "Role created successfully" })
                : new JsonResult(result.Errors);
        }

        // Deletes the role identified by roleId.
        public async Task<IActionResult> DeleteRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return new JsonResult(new { message = "Role not found" });
            }
            var result = await _roleManager.DeleteAsync(role);
            return result.Succeeded
                ? new JsonResult(new { message = "Role deleted successfully" })
                : new JsonResult(result.Errors);
        }

        // Binds a role to a company using the CompanyRole join entity.
        public async Task<IActionResult> BindRoleToCompany(string roleId, Guid companyId)
        {
            var company = await _unitOfWork.CompanyRepository.GetByIdAsync(companyId);
            if (company == null)
            {
                return new JsonResult(new { message = "Company not found" });
            }

            // Check if the role is already bound to the company.
            if (company.CompanyRoles.Any(cr => cr.RoleId == roleId))
            {
                return new JsonResult(new { message = "Role already bound to company" });
            }

            // Create a new join entry.
            var companyRole = new CompanyRole
            {
                CompanyId = company.Id,
                RoleId = roleId
            };

            company.CompanyRoles.Add(companyRole);
            await _unitOfWork.CompanyRepository.UpdateAsync(company);
            await _unitOfWork.SaveChangesAsync();

            return new JsonResult(new { message = "Role bound to company successfully" });
        }

        // Unbinds a role from a company by removing the corresponding CompanyRole entry.
        public async Task<IActionResult> UnbindRoleFromCompany(string roleId, Guid companyId)
        {
            var company = await _unitOfWork.CompanyRepository.GetByIdAsync(companyId);
            if (company == null)
            {
                return new JsonResult(new { message = "Company not found" });
            }

            var companyRole = company.CompanyRoles.FirstOrDefault(cr => cr.RoleId == roleId);
            if (companyRole == null)
            {
                return new JsonResult(new { message = "Role not bound to company" });
            }

            company.CompanyRoles.Remove(companyRole);
            await _unitOfWork.CompanyRepository.UpdateAsync(company);
            await _unitOfWork.SaveChangesAsync();

            return new JsonResult(new { message = "Role unbound from company successfully" });
        }
    }

    // View model for the RoleManagement view component.
    public class RoleManagementViewModel
    {
        public List<IdentityRole> Roles { get; set; } = new List<IdentityRole>();
        public List<CompanyDto> Companies { get; set; } = new List<CompanyDto>();
    }
}
