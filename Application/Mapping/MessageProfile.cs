using Application.DTOs;
using AutoMapper;
using Core.Entity;
using System.Linq;

namespace Application.Mapping;

public class MessageProfile : Profile
{
    public MessageProfile()
    {
        CreateMap<Message, MessageDto>()
            .ForMember(d => d.RecipientIds, opt => opt.MapFrom(s => s.Recipients.Select(r => r.UserId).ToList()));
    }
}

