using Microsoft.EntityFrameworkCore;
using OneSms.Online.Data;
using OneSms.Online.Models;
using OneSms.Web.Shared.Dtos;
using OneSms.Web.Shared.Enumerations;
using OneSms.Web.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OneSms.Online.Services
{
    public class SmsDataExtractorService
    {
        private OneSmsDbContext _oneSmsDbContext;

        public SmsDataExtractorService(OneSmsDbContext oneSmsDbContext)
        {
            _oneSmsDbContext = oneSmsDbContext;
        }

        public async Task<SmsDataToExtract> GetSmsData(SmsReceivedDto smsReceivedDto)
        {
            var sim = (await _oneSmsDbContext.MobileServers.Include(x => x.Sims).FirstAsync(x => x.Key == new Guid(smsReceivedDto.MobileServerKey)))
                .Sims.First(x => x.SimSlot == smsReceivedDto.SimSlot);
            var extractors = await _oneSmsDbContext.SmsDataExtractors.Where(x => x.NetworkId == sim.NetworkId && x.OriginatingAddress == smsReceivedDto.OriginatingAddress).ToListAsync();
            return Extract(smsReceivedDto.Body, sim, extractors);
        }

        public async Task<string> CheckIfIsAirtimeBalanceUpdate(string message, int simId)
        {
            var smsData = new SmsDataToExtract();
            var sim = await _oneSmsDbContext.Sims.FirstAsync(x => x.Id == simId);
            var extractors = await _oneSmsDbContext.SmsDataExtractors.Where(x => x.NetworkId == sim.NetworkId && x.UssdAction == UssdActionType.AirtimeBalance).ToListAsync();
            return Extract(message, sim, extractors)?.Balance;
        }

        public async Task<SmsDataToExtract> GetTimTransactionAsync(SmsReceivedDto smsReceivedDto)
        {
            var extractors = await _oneSmsDbContext.SmsDataExtractors.Where(x => x.UssdAction == UssdActionType.TimTransaction && x.OriginatingAddress == smsReceivedDto.OriginatingAddress).ToListAsync();
            var sim = (await _oneSmsDbContext.MobileServers.Include(x => x.Sims).FirstAsync(x => x.Key == new Guid(smsReceivedDto.MobileServerKey)))
                .Sims.First(x => x.SimSlot == smsReceivedDto.SimSlot);
            var smsData = Extract(smsReceivedDto.Body, sim,extractors);
            smsData.SimId = sim.Id;
            smsData.MobileServerId = sim.MobileServerId;
            return smsData;
        }

        public SmsDataToExtract Extract(string message, SimCard sim, List<SmsDataExtractor> extractors)
        {
            SmsDataToExtract smsData = new SmsDataToExtract();
            foreach (var extractor in extractors)
            {
                if (Regex.IsMatch(message, extractor.RegexPatern, RegexOptions.None))
                {
                    foreach (Match match in Regex.Matches(message, extractor.RegexPatern, RegexOptions.None))
                    {
                        smsData.Minutes = match.Groups["minutes"].Value;
                        smsData.Number = match.Groups["number"].Value;
                        smsData.Cost = match.Groups["cost"].Value;
                        smsData.Balance = match.Groups["balance"].Value;
                    };
                    smsData.SimId = sim.Id;
                    smsData.MobileServerId = sim.MobileServerId;
                    smsData.UssdActionType = extractor.UssdAction;
                    break;
                }
            }
            return smsData;
        }

        private SmsDataToExtract Extract(string message, List<SmsDataExtractor> extractors)
        {
            SmsDataToExtract smsData = new SmsDataToExtract();
            foreach (var extractor in extractors)
            {
                if (Regex.IsMatch(message, extractor.RegexPatern, RegexOptions.None))
                {
                    foreach (Match match in Regex.Matches(message, extractor.RegexPatern, RegexOptions.None))
                    {
                        smsData.Minutes = match.Groups["minutes"].Value;
                        smsData.Number = match.Groups["number"].Value;
                        smsData.Cost = match.Groups["cost"].Value;
                        smsData.Balance = match.Groups["balance"].Value;
                    };
                    smsData.UssdActionType = extractor.UssdAction;
                    break;
                }
            }
            return smsData;
        }
    }
}
