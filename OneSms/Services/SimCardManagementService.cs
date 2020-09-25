using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using OneSms.Contracts.V1.Enumerations;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Data;
using OneSms.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OneSms.Services
{
    public interface ISimCardManagementService
    {
        Task<UssdRequest?> ProcessNetworkMessage(NetworkMessageData networkMessageData, Guid? transactionId = null);
    }

    public class SimCardManagementService : ISimCardManagementService
    {
        private readonly DataContext _dbContext;

        public SimCardManagementService(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UssdRequest?> ProcessNetworkMessage(NetworkMessageData networkMessageData, Guid? transactionId = null)
        {
            if (networkMessageData.NetworkAction == NetworkActionType.SmsBalance && networkMessageData.Amount < 5)
            {
                var simCard = _dbContext.Sims.First(x => x.Id == networkMessageData.SimId);
                var ussdAction = _dbContext.UssdActions.Include(x => x.Steps).FirstOrDefault(x => x.NetworkId == simCard.NetworkId && x.ActionType == networkMessageData.NetworkAction);
                if (ussdAction != null)
                {
                    var ussdTransaction = await AddTransaction(new UssdTransaction
                    {
                        StartTime = DateTime.UtcNow,
                        SimId = simCard.Id,
                        TransactionId = transactionId ?? Guid.NewGuid(),
                        UssdActionId = ussdAction.Id
                    });

                    return new UssdRequest
                    {
                        SimId = networkMessageData.SimId,
                        KeyProblems = ussdAction.KeyProblems.Split(',').ToList(),
                        KeyWelcomes = ussdAction.KeyProblems.Split(',').ToList(),
                        NetworkAction = networkMessageData.NetworkAction,
                        SimSlot = simCard.SimSlot,
                        TransactionId = ussdTransaction.TransactionId,
                        UssdId = ussdTransaction.Id,
                        UssdNumber = ussdAction.UssdNumber,
                        UssdInputs = ussdAction.Steps.OrderBy(x => x.Id).Select(x => x.Value).ToList(),
                        MobileServerId = simCard.MobileServerId
                    };
                }
            }
            return null;
        }

        private async Task<UssdTransaction> AddTransaction(UssdTransaction ussd)
        {
            EntityEntry<UssdTransaction> created = _dbContext.UssdTransactions.Add(ussd);
            await _dbContext.SaveChangesAsync();
            ussd.Id = created.Entity.Id;
            return ussd;
        }

    }
}
