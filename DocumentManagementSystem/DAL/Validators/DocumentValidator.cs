using System.ComponentModel;
using FluentValidation;
using DocumentManagementSystem.Entities;

namespace DAL.Validators;

public class DocumentValidator : AbstractValidator<Document>
{
    public DocumentValidator()
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