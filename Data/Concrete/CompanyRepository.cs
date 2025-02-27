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
        public async Task<Company?> GetByIdWithToolsAsync(Guid id)
        {
            return await _dbSet
                .Include(c => c.Tools)
                    .ThenInclude(t => t.Topics)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<List<Company>> GetAllWithToolsAndTopicsAsync()
        {
            return await _dbSet
                .Include(c => c.Topics)         // Company-level topics
                .Include(c => c.Tools)
                    .ThenInclude(t => t.Topics)  // Tool-level topics
                .Include(c => c.CompanyRoles)    // Roles if needed
                .ToListAsync();
        }


    }
}
