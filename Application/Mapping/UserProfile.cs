using Application.DTOs;
using AutoMapper;
using Core.Entity;

namespace Application.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.Roles != null ? src.Roles.Select(r => r.Role.ToString()).ToList() : new List<string>()));

        CreateMap<User, UserDetailDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                src.Roles != null ? src.Roles.Select(r => r.Role.ToString()).ToList() : new List<string>()));
    }
}
