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

        // Add MqttTopic-specific methods here if needed
    }
}
