using OneSms.Web.Shared.Models;

namespace OneSms.Web.Server.Data.Repositories
{
    public class NetworkOperatorRepository: BaseRepository<NetworkOperator>
    {
        public NetworkOperatorRepository(OneSmsDbContext dbContext) : base(dbContext)
        {
        }
    }
    public interface INetworkOperatorRepository : IBaseRepository<NetworkOperator>
    {
        
    }
}
