using AutoMapper;
using Xunit;
using DocumentManagementSystem.DTOs;
using DAL.Entities;

public class MappingTests
{
    private readonly IMapper _mapper;

    public MappingTests()
    {
        var mappingConfig = new MapperConfiguration(mc => {
            mc.AddProfile(new MappingProfile());
        });
        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void TestMapping_UserDTOToUser()
    {
        // Arrange
        var documentDto = new DocumentDTO { Id = 0, Name = "PFD", Path = "root/var" };

        // Act
        var document = _mapper.Map<Document>(documentDto);

        // Assert
        Assert.Equal(documentDto.Id, document.Id);
        Assert.Equal(documentDto.Name, document.Name);
        Assert.Equal(documentDto.Path, document.Path);
    }
}