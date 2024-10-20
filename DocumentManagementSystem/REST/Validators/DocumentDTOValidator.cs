using DocumentManagementSystem.DTOs;
using FluentValidation;

namespace DocumentManagementSystem.Validators
{
    public class DocumentDTOValidator : AbstractValidator<DocumentDTO>
    {
        public DocumentDTOValidator()
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
}