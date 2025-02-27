using Final.Entity;

namespace Final.Data.Abstract
{
    public interface IMqttToolRepository : IRepository<MqttTool>
    {
        // Add specialized methods for MqttTool if needed
        Task<IEnumerable<MqttTool>> GetToolsByCompanyIdAsync(Guid companyId);
    }
}
