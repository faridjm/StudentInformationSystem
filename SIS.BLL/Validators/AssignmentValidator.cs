using FluentValidation;
using SIS.BLL.Services.DTOs.Assignment;

namespace SIS.BLL.Validators
{
    public class CreateAssignmentDtoValidator : AbstractValidator<CreateAssignmentDto>
    {
        public CreateAssignmentDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(2000);
            RuleFor(x => x.MaxScore).GreaterThan(0).LessThanOrEqualTo(1000);
            RuleFor(x => x.DueDate).GreaterThan(DateTime.UtcNow.AddMinutes(-5))
                .WithMessage("Son tarix keçmiş ola bilməz");
            RuleFor(x => x.CourseOfferingId).GreaterThan(0);
            RuleFor(x => x.Type).IsInEnum();
        }
    }

    public class GradeSubmissionDtoValidator : AbstractValidator<GradeSubmissionDto>
    {
        public GradeSubmissionDtoValidator()
        {
            RuleFor(x => x.SubmissionId).GreaterThan(0);
            RuleFor(x => x.Score).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Feedback).MaximumLength(1000);
        }
    }
}
