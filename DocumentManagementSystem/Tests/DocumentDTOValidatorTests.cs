using DocumentManagementSystem.DTOs;
using DocumentManagementSystem.Validators;
using FluentValidation.TestHelper;
using Xunit;

public class DocumentDTOValidatorTests
{
    private readonly DocumentDTOValidator _validator;

    public DocumentDTOValidatorTests()
    {
        _validator = new DocumentDTOValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var model = new DocumentDTO { Name = "", Path = "some/path", FileType = ".pdf" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(doc => doc.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Should_Have_Error_When_FileType_Is_Empty()
    {
        var model = new DocumentDTO { Name = "Test", Path = "some/path", FileType = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(doc => doc.FileType)
            .WithErrorMessage("FileType is required");
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var model = new DocumentDTO { Name = "Test", Path = "some/path", FileType = ".pdf" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(doc => doc.Name);
        result.ShouldNotHaveValidationErrorFor(doc => doc.Path);
        result.ShouldNotHaveValidationErrorFor(doc => doc.FileType);
    }
}