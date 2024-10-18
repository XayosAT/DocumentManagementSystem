using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using DAL.Entities;

public class UserValidationTests
{
    /*
    [Fact]
    public void ValidateUser_ValidData_ReturnsTrue()
    {
        // Arrange
        var document = new Document { Id = 0, Name = "PDF", Path = "root/var" };
        var context = new ValidationContext(document, null, null);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(document, context, results, true);

        // Assert
        Assert.True(valid);
    }

    [Fact]
    public void ValidateUser_InvalidEmail_ReturnsFalse()
    {
        // Arrange
        var user = new Document { Name = "John", Path = "Doe"};
        var context = new ValidationContext(user, null, null);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(user, context, results, true);

        // Assert
        Assert.False(valid);
    }
    */
}