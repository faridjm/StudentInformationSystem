using FluentValidation;
using SIS.BLL.Services.DTOs.AcademicYear;

namespace SIS.BLL.Validators
{
    public class CreateAcademicYearDtoValidator : AbstractValidator<CreateAcademicYearDto>
    {
        public CreateAcademicYearDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Sezon adı boş ola bilməz")
                .MaximumLength(20).WithMessage("Sezon adı 20 simvoldan çox ola bilməz");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Başlama tarixi boş ola bilməz");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("Bitmə tarixi boş ola bilməz")
                .GreaterThan(x => x.StartDate)
                .WithMessage("Bitmə tarixi başlama tarixindən sonra olmalıdır");
        }
    }
}
