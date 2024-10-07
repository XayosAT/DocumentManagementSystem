using AutoMapper;
using DAL.Entities;

namespace DocumentManagementSystem.DTOs;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDTO>();
        CreateMap<UserDTO, User>();
    }
}