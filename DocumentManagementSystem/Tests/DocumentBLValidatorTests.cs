using REST.Validators;
using FluentValidation.TestHelper;
using SharedData.EntitiesBL;
using Xunit;

public class DocumentBLValidatorTests
{
    private readonly DocumentBLValidator _validator;

    public DocumentBLValidatorTests()
    {
        _validator = new DocumentBLValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var model = new DocumentBL { Name = "", Path = "some/path", FileType = "pdf" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(doc => doc.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Should_Have_Error_When_FileType_Is_Empty()
    {
        // Arrange
        var model = new DocumentBL { Name = "Test", Path = "some/path", FileType = "" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(doc => doc.FileType)
            .WithErrorMessage("FileType is required");
    }

    [Fact]
    public void Should_Have_Error_When_Path_Is_Empty()
    {
        // Arrange
        var model = new DocumentBL { Name = "Test", Path = "", FileType = ".pdf" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(doc => doc.Path)
            .WithErrorMessage("Path is required");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var model = new DocumentBL { Name = "Test", Path = "some/path", FileType = ".pdf" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(doc => doc.Name);
        result.ShouldNotHaveValidationErrorFor(doc => doc.Path);
        result.ShouldNotHaveValidationErrorFor(doc => doc.FileType);
    }
}