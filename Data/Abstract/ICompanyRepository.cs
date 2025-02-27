using System.Threading.Tasks;
using Final.Entity;

namespace Final.Data.Abstract
{
    public interface ICompanyRepository : IRepository<Company>
    {
        Task<Company?> GetCompanyByNameAsync(string name);
        Task<Company?> GetByIdWithToolsAsync(Guid id);
        Task<List<Company>> GetAllWithToolsAndTopicsAsync();
    }
}
