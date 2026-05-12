using FluentValidation;
using SIS.BLL.Services.DTOs.Enrollment;

namespace SIS.BLL.Validators
{
    public class CreateEnrollmentDtoValidator : AbstractValidator<CreateEnrollmentDto>
    {
        public CreateEnrollmentDtoValidator()
        {
            RuleFor(x => x.StudentId)
                .GreaterThan(0).WithMessage("Tələbə seçilməlidir");

            RuleFor(x => x.CourseOfferingId)
                .GreaterThan(0).WithMessage("Kurs təklifi seçilməlidir");
        }
    }
}
