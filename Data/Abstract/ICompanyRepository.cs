using System.Threading.Tasks;
using Final.Entity;

namespace Final.Data.Abstract
{
    public interface ICompanyRepository : IRepository<Company>
    {
        // Example specialized method
        Task<Company?> GetCompanyByNameAsync(string name);
    }
}
