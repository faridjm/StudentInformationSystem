using FluentValidation;
using SIS.BLL.Services.DTOs.Course;

namespace SIS.BLL.Validators
{
    public class CreateCourseDtoValidator : AbstractValidator<CreateCourseDto>
    {
        public CreateCourseDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Kurs adı boş ola bilməz");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Kurs kodu boş ola bilməz")
                .Matches("^[A-Z]{2,4}[0-9]{3}$").WithMessage("Kurs kodu düzgün formatda deyil (məs: CS101)");

            RuleFor(x => x.Credits)
                .InclusiveBetween(1, 10).WithMessage("Kredit 1-10 arasında olmalıdır");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Departament seçilməlidir");
        }
    }

}
