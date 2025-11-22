using Application.DTOs;
using AutoMapper;
using Core.Entity;

namespace Application.Mapping
{
    public class GroupProfile : Profile
    {
        public GroupProfile()
        {
            CreateMap<Group, GroupDto>();
        }
    }
}
