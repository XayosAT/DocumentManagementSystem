using System.ComponentModel;
using FluentValidation;
using SharedData.EntitiesDAL;

namespace REST.Validators;

public class DocumentDALValidator : AbstractValidator<DocumentDAL>
{
    public DocumentDALValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name is too long");

        RuleFor(x => x.Path)
            .NotEmpty().WithMessage("Path is required");

        RuleFor(x => x.FileType)
            .NotEmpty().WithMessage("FileType is required");
    }
}