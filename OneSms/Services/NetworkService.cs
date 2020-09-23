using Microsoft.EntityFrameworkCore;
using OneSms.Data;
using OneSms.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneSms.Services
{
    public interface INetworkService
    {
        Task<List<NetworkOperator>> GetNetworks();
    }

    public class NetworkService : INetworkService
    {
        private readonly DataContext _dbContext;

        public NetworkService(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<List<NetworkOperator>> GetNetworks()
        {
            return _dbContext.Networks.ToListAsync();
        }
    }
}
