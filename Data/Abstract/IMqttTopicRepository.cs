using Final.Entity;

namespace Final.Data.Abstract
{
    public interface IMqttTopicRepository : IRepository<MqttTopic>
    {
        // Existing method:
        Task<IEnumerable<MqttTopic>> GetByCompanyIdAndNoToolAsync(Guid companyId);

        // New method: Get all topics for a specific tool.
        Task<IEnumerable<MqttTopic>> GetTopicsByToolIdAsync(Guid toolId);

        // New method: Get all topics for a specific company.
        Task<IEnumerable<MqttTopic>> GetTopicsByCompanyIdAsync(Guid companyId);

    }
}
