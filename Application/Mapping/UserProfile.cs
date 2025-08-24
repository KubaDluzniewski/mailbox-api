using Application.DTOs;
using AutoMapper;
using Core.Entity;

namespace Application.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();
    }
}
