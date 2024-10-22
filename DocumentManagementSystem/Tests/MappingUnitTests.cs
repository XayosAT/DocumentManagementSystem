using AutoMapper;
using Xunit;
using SharedData;
using SharedData.DTOs;
using SharedData.EntitiesBL;
using SharedData.EntitiesDAL;

public class MappingTests
{
    private readonly IMapper _mapper;

    public MappingTests()
    {
        var mappingConfig = new MapperConfiguration(mc => {
            mc.AddProfile(new DocumentMappingProfile());
        });
        _mapper = mappingConfig.CreateMapper();
    }

    [Fact]
    public void TestMapping_DocumentDTOToDocument_Success()
    {
        // Arrange
        var documentDto = new DocumentDTO { Id = 1, Name = "PDF", Path = "root/var" };

        // Act
        var document = _mapper.Map<DocumentDAL>(documentDto);

        // Assert
        Assert.Equal(documentDto.Id, document.Id);
        Assert.Equal(documentDto.Name, document.Name);
        Assert.Equal(documentDto.Path, document.Path);
    }

    [Fact]
    public void TestMapping_DocumentDTOToDocumentBL_Success()
    {
        // Arrange
        var documentDto = new DocumentDTO { Id = 1, Name = "PDF", Path = "root/var", FileType = "pdf" };

        // Act
        var documentBL = _mapper.Map<DocumentBL>(documentDto);

        // Assert
        Assert.Equal(documentDto.Id, documentBL.Id);
        Assert.Equal(documentDto.Name, documentBL.Name);
        Assert.Equal(documentDto.Path, documentBL.Path);
        Assert.Equal(documentDto.FileType, documentBL.FileType);
    }

    [Fact]
    public void TestMapping_DocumentBLToDocumentDAL_Success()
    {
        // Arrange
        var documentBL = new DocumentBL { Id = 1, Name = "PDF", Path = "root/var", FileType = "pdf" };

        // Act
        var documentDAL = _mapper.Map<DocumentDAL>(documentBL);

        // Assert
        Assert.Equal(documentBL.Id, documentDAL.Id);
        Assert.Equal(documentBL.Name, documentDAL.Name);
        Assert.Equal(documentBL.Path, documentDAL.Path);
        Assert.Equal(documentBL.FileType, documentDAL.FileType);
    }
}