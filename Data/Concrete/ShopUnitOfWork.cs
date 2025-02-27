using System;
using System.Threading.Tasks;
using Final.Data.Abstract;

namespace Final.Data.Concrete
{
    public class ShopUnitOfWork : IShopUnitOfWork
    {
        private readonly ShopContext _context;

        // Backing fields for lazy initialization
        private CompanyRepository? _companyRepository;
        private MqttToolRepository? _mqttToolRepository;
        private MqttTopicRepository? _mqttTopicRepository;

        public ShopUnitOfWork(ShopContext context)
        {
            _context = context;
        }

        public ICompanyRepository CompanyRepository 
            => _companyRepository ??= new CompanyRepository(_context);

        public IMqttToolRepository MqttToolRepository
            => _mqttToolRepository ??= new MqttToolRepository(_context);

        public IMqttTopicRepository MqttTopicRepository
            => _mqttTopicRepository ??= new MqttTopicRepository(_context);

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        // Expose the underlying context.
        public ShopContext Context => _context;

        #region IDisposable Implementation
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
