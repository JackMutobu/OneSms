using AutoMapper;
using OneSms.Contracts.V1.MobileServerRequest;
using OneSms.Domain;
using System.Collections.Generic;

namespace OneSms.Mappings
{
    public class DomainToMobileServerRequest: Profile
    {
        public DomainToMobileServerRequest()
        {
            CreateMap<SmsMessage, SmsRequest>()
                 .ForMember(dest => dest.SimSlot, opt => opt.Ignore())
                 .ForMember(dest => dest.ReceiverNumber, opt => opt.MapFrom(src => src.RecieverNumber))
                .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.Id));

            CreateMap<WhatsappMessage, WhatsappRequest>()
               .ForMember(dest => dest.MessageId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ReceiverNumber, opt => opt.MapFrom(src => src.RecieverNumber))
               .ForMember(dest => dest.ImageLinks, opt =>
                   opt.MapFrom(src => new List<string> { src.ImageLinkOne, src.ImageLinkTwo, src.ImageLinkThree })); ;
        }
    }
}
