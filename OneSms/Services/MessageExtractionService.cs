using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OneSms.Data;
using OneSms.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OneSms.Services
{
    public interface IMessageExtractionService
    {
        Task<NetworkMessageExtractor> AddExtractor(NetworkMessageExtractor messageExtractor);
        Task<List<NetworkMessageExtractor>> GetExtractors();
        Task<int> DeleteExtractor(NetworkMessageExtractor messageExtractor);
    }

    public class MessageExtractionService : IMessageExtractionService
    {
        private readonly DataContext _dbContext;

        public MessageExtractionService(DataContext dataContext)
        {
            _dbContext = dataContext;
        }

        public async Task<NetworkMessageExtractor> AddExtractor(NetworkMessageExtractor messageExtractor)
        {
            EntityEntry<NetworkMessageExtractor> created = _dbContext.NetworkMessageExtractors.Update(messageExtractor);
            await _dbContext.SaveChangesAsync();
            messageExtractor.Id = created.Entity.Id;
            return messageExtractor;
        }

        public Task<List<NetworkMessageExtractor>> GetExtractors()
        {
            return _dbContext.NetworkMessageExtractors.Include(x => x.Network).ToListAsync();
        }

        public Task<int> DeleteExtractor(NetworkMessageExtractor messageExtractor)
        {
            _dbContext.NetworkMessageExtractors.Remove(messageExtractor);
            return _dbContext.SaveChangesAsync();
        }
    }
}
