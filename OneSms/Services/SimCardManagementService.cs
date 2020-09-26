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
        Task<UssdRequest?> GetUssdRequest(SmsRequest smsRequest);
    }

    public class SimCardManagementService : ISimCardManagementService
    {
        private readonly DataContext _dbContext;

        public SimCardManagementService(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<UssdRequest?> ProcessNetworkMessage(NetworkMessageData networkMessageData, Guid? transactionId = null)
        {
            var simCard = _dbContext.Sims.First(x => x.Id == networkMessageData.SimId);
            if (networkMessageData.NetworkAction == NetworkActionType.SmsBalance && networkMessageData.Amount < simCard.MinSmsBalance)
                return GetUssdRequest(simCard, NetworkActionType.AirtimeBalance, transactionId);
            else if (networkMessageData.NetworkAction == NetworkActionType.AirtimeBalance)
            {
                if (decimal.Parse(simCard.SmsBalance ?? "0") < simCard.MinSmsBalance)
                {
                    if (decimal.Parse(simCard.AirtimeBalance ?? "0") >= simCard.MinAirtimeBalance)
                        return GetUssdRequest(simCard, NetworkActionType.SmsActivation, transactionId);
                    else
                        return GetUssdRequest(simCard, NetworkActionType.AirtimeRecharge, transactionId);
                }
            }
            else if(networkMessageData.NetworkAction == NetworkActionType.AirtimeRecharge)
                return GetUssdRequest(simCard, NetworkActionType.SmsActivation, transactionId);

            return Task.FromResult<UssdRequest?>(null);
        }
        public Task<UssdRequest?> GetUssdRequest(SmsRequest smsRequest)
        {
            var simCard = _dbContext.Sims.First(x => x.MobileServerId == smsRequest.MobileServerId && x.SimSlot == smsRequest.SimSlot);
            return GetUssdRequest(simCard, NetworkActionType.SmsBalance);
        }
        private async Task<UssdRequest?> GetUssdRequest(SimCard simCard, NetworkActionType networkActionType, Guid? transactionId = null)
        {
            var ussdAction = _dbContext.UssdActions.Include(x => x.Steps).FirstOrDefault(x => x.NetworkId == simCard.NetworkId && x.ActionType == networkActionType);
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
                    SimId = simCard.Id,
                    KeyProblems = ussdAction.KeyProblems.Split(',').ToList(),
                    KeyWelcomes = ussdAction.KeyLogins.Split(',').ToList(),
                    NetworkAction = ussdAction.ActionType,
                    SimSlot = simCard.SimSlot,
                    TransactionId = ussdTransaction.TransactionId,
                    UssdId = ussdTransaction.Id,
                    UssdNumber = ussdAction.UssdNumber,
                    UssdInputs = ussdAction.Steps.OrderBy(x => x.Id).Select(x => x.Value).ToList(),
                    MobileServerId = simCard.MobileServerId
                };
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
