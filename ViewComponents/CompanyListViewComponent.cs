using System.Threading.Tasks;
using Final.Data.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Final.ViewComponents
{
    public class CompanyListViewComponent : ViewComponent
    {
        private readonly IShopUnitOfWork _unitOfWork;

        public CompanyListViewComponent(IShopUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Fetches all companies and renders them using the default view.
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var companies = await _unitOfWork.CompanyRepository.GetAllAsync();
            return View(companies);
        }
    }
}
