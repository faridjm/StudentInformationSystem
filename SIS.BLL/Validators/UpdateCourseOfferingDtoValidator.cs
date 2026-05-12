using FluentValidation;
using SIS.BLL.Services.DTOs.CourseOffering;

namespace SIS.BLL.Validators
{
    public class UpdateCourseOfferingDtoValidator : AbstractValidator<UpdateCourseOfferingDto>
    {
        public UpdateCourseOfferingDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id düzgün deyil");

            RuleFor(x => x.TeacherId)
                .GreaterThan(0).WithMessage("Müəllim seçilməlidir");

            RuleFor(x => x.Capacity)
                .InclusiveBetween(1, 200).WithMessage("Tutum 1-200 arasında olmalıdır");
        }
    }
}
