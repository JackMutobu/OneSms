using Microsoft.AspNetCore.Rewrite;
using OneSms.Contracts.V1.Dtos;
using OneSms.Contracts.V1.Enumerations;
using OneSms.Data;
using OneSms.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OneSms.Services
{
    public interface INetworkMessageExtractionService
    {
        NetworkMessageData? GetMessageData(SmsReceived smsReceived);
        Task<int> SaveNetworkMessageData(NetworkMessageData networkMessage);
    }

    public class NetworkMessageExtractionService : INetworkMessageExtractionService
    {
        private readonly DataContext _dbContext;

        public NetworkMessageExtractionService(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public NetworkMessageData? GetMessageData(SmsReceived smsReceived)
        {
            var simCard = _dbContext.Sims
                .FirstOrDefault(x => x.SimSlot == smsReceived.SimSlot && x.MobileServerId == new Guid(smsReceived.MobileServerKey));
            if (simCard != null)
            {
                var messageDataExtractors = _dbContext.NetworkMessageExtractors
                    .Where(x => x.OriginatingAddress.ToLower() == smsReceived.SenderNumber.ToLower() && x.NetworkId == simCard.NetworkId).ToList();
                return Extract(smsReceived.Body, simCard, messageDataExtractors);
            }
            return null;
        }

        public async Task<int> SaveNetworkMessageData(NetworkMessageData networkMessage)
        {
            var simCard = _dbContext.Sims.FirstOrDefault(x => x.Id == networkMessage.SimId);
            if (simCard != null)
            {
                switch (networkMessage.NetworkAction)
                {
                    case NetworkActionType.AirtimeBalance:
                        simCard.AirtimeBalance = networkMessage.Amount.ToString();
                        break;
                    case NetworkActionType.AirtimeRecharge:
                        simCard.AirtimeBalance = (decimal.Parse(simCard.AirtimeBalance ?? "0") + networkMessage.Amount).ToString();
                        break;
                    case NetworkActionType.SmsBalance:
                        simCard.SmsBalance = networkMessage.Amount.ToString();
                        break;
                    case NetworkActionType.SmsActivation:
                        simCard.SmsBalance = (decimal.Parse(simCard.SmsBalance ?? "0") + networkMessage.Amount).ToString();
                        break;
                    case NetworkActionType.MobileMoneyBalance:
                        simCard.MobileMoneyBalance = networkMessage.Amount.ToString();
                        break;
                }

                _dbContext.Update(simCard);
                _dbContext.Add(networkMessage);
                return await _dbContext.SaveChangesAsync();
            }
            return 0;
        }

        private NetworkMessageData? Extract(string message, SimCard sim, List<NetworkMessageExtractor> extractors)
        {
            foreach (var extractor in extractors)
            {
                if (Regex.IsMatch(message, extractor.RegexPatern, RegexOptions.None))
                {
                    var messageData = new NetworkMessageData();
                    foreach (Match? match in Regex.Matches(message, extractor.RegexPatern, RegexOptions.None))
                    {
                        if (match != null)
                        {
                            var amount = match.Groups["amount"].Value;
                            if (!string.IsNullOrEmpty(amount))
                            {
                                messageData.Amount = decimal.Parse(amount);
                            }
                            else
                                messageData.Amount = 0;

                           var cost = match.Groups["cost"].Value;
                            if(!string.IsNullOrEmpty(cost))
                            {
                                messageData.Cost = decimal.Parse(cost);
                            }
                        }
                    };
                    messageData.SimId = sim.Id;
                    messageData.Message = message;
                    messageData.NetworkAction = extractor.NetworkAction;
                    messageData.ExecutionDate = DateTime.UtcNow;
                    return messageData;
                }
            }
            return null;
        }
    }
}
