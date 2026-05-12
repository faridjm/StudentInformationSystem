using FluentValidation;
using SIS.BLL.Services.DTOs.CourseOffering;

namespace SIS.BLL.Validators
{
    public class CreateCourseOfferingDtoValidator : AbstractValidator<CreateCourseOfferingDto>
    {
        public CreateCourseOfferingDtoValidator()
        {
            RuleFor(x => x.CourseId)
                .GreaterThan(0).WithMessage("Kurs seçilməlidir");

            RuleFor(x => x.AcademicTermId)
                .GreaterThan(0).WithMessage("Termin seçilməlidir");

            RuleFor(x => x.TeacherId)
                .GreaterThan(0).WithMessage("Müəllim seçilməlidir");

            RuleFor(x => x.Capacity)
                .InclusiveBetween(1, 200).WithMessage("Tutum 1-200 arasında olmalıdır");
        }
    }
}
