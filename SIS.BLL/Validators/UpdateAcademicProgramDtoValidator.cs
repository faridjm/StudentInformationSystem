using FluentValidation;
using SIS.BLL.Services.DTOs.AcademicProgram;

namespace SIS.BLL.Validators
{
    public class UpdateAcademicProgramDtoValidator : AbstractValidator<UpdateAcademicProgramDto>
    {
        public UpdateAcademicProgramDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id düzgün deyil");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ad boş ola bilməz");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Kod boş ola bilməz");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Departament seçilməlidir");

            RuleFor(x => x.DurationYears)
                .InclusiveBetween(1, 6).WithMessage("Müddət 1-6 il arasında olmalıdır");
        }
    }
}
