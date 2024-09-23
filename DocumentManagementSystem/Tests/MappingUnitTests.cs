using AutoMapper;
using Xunit;
using DocumentManagementSystem.DTOs;
using DocumentManagementSystem.Entities;

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
        var userDto = new UserDTO { FirstName = "John", LastName = "Doe", Email = "john@example.com" };

        // Act
        var user = _mapper.Map<User>(userDto);

        // Assert
        Assert.Equal(userDto.FirstName, user.FirstName);
        Assert.Equal(userDto.LastName, user.LastName);
        Assert.Equal(userDto.Email, user.Email);
    }
}