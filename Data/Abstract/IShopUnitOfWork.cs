using System;
using System.Threading.Tasks;

namespace Final.Data.Abstract
{
    public interface IShopUnitOfWork : IDisposable
    {
        ICompanyRepository CompanyRepository { get; }
        IMqttToolRepository MqttToolRepository { get; }
        IMqttTopicRepository MqttTopicRepository { get; }

        Task SaveChangesAsync();
    }
}
