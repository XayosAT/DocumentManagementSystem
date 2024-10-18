using AutoMapper;
using DocumentManagementSystem.Entities;

namespace DocumentManagementSystem.DTOs;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Document, DocumentDTO>();
        CreateMap<DocumentDTO, Document>();
    }
}