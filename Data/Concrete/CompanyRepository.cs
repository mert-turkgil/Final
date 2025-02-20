using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Final.Data.Abstract;
using Final.Entity;

namespace Final.Data.Concrete
{
    public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
    {
        public CompanyRepository(ShopContext context) : base(context)
        {
        }

        public async Task<Company?> GetCompanyByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Name == name);
        }
    }
}
