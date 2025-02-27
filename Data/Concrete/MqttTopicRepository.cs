using Final.Data.Abstract;
using Final.Entity;
using Microsoft.EntityFrameworkCore;

namespace Final.Data.Concrete
{
    public class MqttTopicRepository : RepositoryBase<MqttTopic>, IMqttTopicRepository
    {
        public MqttTopicRepository(ShopContext context) : base(context)
        {
        }

        public async Task<IEnumerable<MqttTopic>> GetByCompanyIdAndNoToolAsync(Guid companyId)
        {
            return await _dbSet
                .Where(t => t.CompanyId == companyId && t.MqttToolId == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<MqttTopic>> GetTopicsByToolIdAsync(Guid toolId)
        {
            return await _dbSet
                .Where(t => t.MqttToolId == toolId)
                .ToListAsync();
        }

        public async Task<IEnumerable<MqttTopic>> GetTopicsByCompanyIdAsync(Guid companyId)
        {
            return await _dbSet
                .Where(t => t.CompanyId == companyId)
                .ToListAsync();
        }

    }
}
