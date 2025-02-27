using Final.Data.Abstract;
using Final.Entity;
using Microsoft.EntityFrameworkCore;

namespace Final.Data.Concrete
{
    public class MqttToolRepository : RepositoryBase<MqttTool>, IMqttToolRepository
    {
        public MqttToolRepository(ShopContext context) : base(context)
        {
        }
        public async Task<IEnumerable<MqttTool>> GetToolsByCompanyIdAsync(Guid companyId)
        {
            return await _dbSet
                .Where(t => t.CompanyId == companyId)
                .ToListAsync();
        }

        // Add MqttTool-specific methods here if needed
    }
}
