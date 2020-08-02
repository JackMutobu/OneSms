using DynamicData.Annotations;
using OneSms.Web.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OneSms.Online.Models
{
    public class SimAdminDto:SimCard
    {
        public SimAdminDto()
        {
            AppIds = new List<Guid>();
        }
        public SimAdminDto(SimCard simCard)
        {
            Id = simCard.Id;
            Name = simCard.Name;
            Number = simCard.Number;
            UssdTransactions = simCard.UssdTransactions;
            Apps = simCard.Apps;
            AirtimeBalance = simCard.AirtimeBalance;
            SmsBalance = simCard.SmsBalance;
            MobileMoneyBalance = simCard.MobileMoneyBalance;
            CreatedOn = simCard.CreatedOn;
            Network = simCard.Network;
            NetworkId = simCard.NetworkId;
            AppIds = simCard.Apps.Select(x => x.AppId).ToList();
            SimSlot = simCard.SimSlot;
            MobileServerId = simCard.MobileServerId;
            
        }

        [Required]
        public List<Guid> AppIds { get; set; }

        public SimCard GetSimCard()
        { 
            return new SimCard
            {
                SmsBalance = this.SmsBalance,
                AirtimeBalance = this.AirtimeBalance,
                CreatedOn = this.CreatedOn,
                Id = this.Id,
                MobileMoneyBalance = this.MobileMoneyBalance,
                Name = this.Name,
                Network = this.Network,
                NetworkId = this.NetworkId,
                Number = this.Number,
                UssdTransactions = this.UssdTransactions,
                Apps = this.Apps,
                SimSlot = this.SimSlot,
                MobileServerId = this.MobileServerId
        };
        }
    }
}
