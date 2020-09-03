using OneSms.Data;
using System.Linq;
using System.Threading.Tasks;

namespace OneSms.Services
{
    public class SimService
    {
        private OneSmsDbContext _oneSmsDbContext;
        private SmsDataExtractorService _smsDataExtractorService;

        public SimService(OneSmsDbContext oneSmsDbContext,SmsDataExtractorService smsDataExtractorService)
        {
            _oneSmsDbContext = oneSmsDbContext;
            _smsDataExtractorService = smsDataExtractorService;
        }

        public async Task CheckIfIsBalanceUpdate(string message, int simId)
        {
            var balance = await _smsDataExtractorService.CheckIfIsAirtimeBalanceUpdate(message, simId);
            if (!string.IsNullOrEmpty(balance))
            {
                var sim = _oneSmsDbContext.Sims.First(x => x.Id == simId);
                sim.AirtimeBalance = balance;
                _oneSmsDbContext.Update(sim);
                await _oneSmsDbContext.SaveChangesAsync();
            }
        }
    }
}
