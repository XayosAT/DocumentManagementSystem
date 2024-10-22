using FluentValidation;
using SharedData.EntitiesBL;

namespace REST.Validators
{
    public class DocumentBLValidator : AbstractValidator<DocumentBL>
    {
        public DocumentBLValidator()
        {
            // Validation rules for the Name property
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name is too long");

            // Validation rules for the Path property
            RuleFor(x => x.Path)
                .NotEmpty().WithMessage("Path is required");

            // Validation rules for the FileType property
            RuleFor(x => x.FileType)
                .NotEmpty().WithMessage("FileType is required");

            // Additional Business Rule: Example
            // Ensure the FileType is either '.pdf', '.docx', or '.txt'
            RuleFor(x => x.FileType)
                .Must(fileType => new[] { ".pdf", ".docx", ".txt" }.Contains(fileType.ToLower()))
                .WithMessage("FileType must be either '.pdf', '.docx', or '.txt'");
        }
    }
}