using AutoMapper;
using OneSms.Contracts.V1.Responses;
using OneSms.Domain;
using System.Collections.Generic;

namespace OneSms.Mappings
{
    public class DomainToResponseProfile: Profile
    {
        public DomainToResponseProfile()
        {
            CreateMap<BaseMessage, MessageResponse>()
                .ForMember(dest => dest.Images, opt => opt.Ignore());

            CreateMap<SmsMessage, MessageResponse>()
                .ForMember(dest => dest.Images,opt => opt.Ignore());

            CreateMap<WhatsappMessage, MessageResponse>()
               .ForMember(dest => dest.Images, opt =>
                   opt.MapFrom(src => new List<string> { src.ImageLinkOne,src.ImageLinkTwo, src.ImageLinkThree }));
        }
    }
}
