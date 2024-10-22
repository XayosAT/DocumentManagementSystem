using AutoMapper;
using SharedData.DTOs;
using SharedData.EntitiesDAL;
using SharedData.EntitiesBL;

namespace SharedData
{
    public class DocumentMappingProfile : Profile
    {
        public DocumentMappingProfile()
        {
            // Mapping between DTO and DAL entities
            CreateMap<DocumentDTO, DocumentDAL>().ReverseMap();

            // Mapping between DTO and Business Layer entities
            CreateMap<DocumentDTO, DocumentBL>().ReverseMap();

            // Mapping between Business Layer and DAL entities
            CreateMap<DocumentBL, DocumentDAL>().ReverseMap();
        }
    }
}