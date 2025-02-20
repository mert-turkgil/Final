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

        // Add MqttTool-specific methods here if needed
    }
}
