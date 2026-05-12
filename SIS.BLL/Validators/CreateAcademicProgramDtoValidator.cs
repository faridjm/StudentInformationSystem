using FluentValidation;
using SIS.BLL.Services.DTOs.AcademicProgram;


namespace SIS.BLL.Validators
{ 

    public class CreateAcademicProgramDtoValidator : AbstractValidator<CreateAcademicProgramDto>
    {
        public CreateAcademicProgramDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ad boş ola bilməz");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Kod boş ola bilməz")
                .MaximumLength(10).WithMessage("Kod maksimum 10 simvol ola bilər");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Departament seçilməlidir");

            RuleFor(x => x.DurationYears)
                .InclusiveBetween(1, 6).WithMessage("Müddət 1-6 il arasında olmalıdır");
        }
    }
}
