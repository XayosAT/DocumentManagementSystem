using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Xunit;
using DocumentManagementSystem.Entities;

public class UserValidationTests
{
    [Fact]
    public void ValidateUser_ValidData_ReturnsTrue()
    {
        // Arrange
        var user = new User { FirstName = "John", LastName = "Doe", Email = "john@example.com" };
        var context = new ValidationContext(user, null, null);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(user, context, results, true);

        // Assert
        Assert.True(valid);
    }

    [Fact]
    public void ValidateUser_InvalidEmail_ReturnsFalse()
    {
        // Arrange
        var user = new User { FirstName = "John", LastName = "Doe"};
        var context = new ValidationContext(user, null, null);
        var results = new List<ValidationResult>();

        // Act
        bool valid = Validator.TryValidateObject(user, context, results, true);

        // Assert
        Assert.False(valid);
    }
}