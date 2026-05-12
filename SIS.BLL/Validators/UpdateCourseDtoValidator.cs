using FluentValidation;
using SIS.BLL.Services.DTOs.Course;

namespace SIS.BLL.Validators
{
    public class UpdateCourseDtoValidator : AbstractValidator<UpdateCourseDto>
    {
        public UpdateCourseDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id düzgün deyil");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Kurs adı boş ola bilməz");

            RuleFor(x => x.Credits)
                .InclusiveBetween(1, 10).WithMessage("Kredit 1-10 arasında olmalıdır");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Departament seçilməlidir");
        }
    }

}
