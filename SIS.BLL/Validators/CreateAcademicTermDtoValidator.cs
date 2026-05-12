using FluentValidation;
using SIS.BLL.Services.DTOs.AcademicTerm;

namespace SIS.BLL.Validators
{

    public class CreateAcademicTermDtoValidator : AbstractValidator<CreateAcademicTermDto>
    {
        public CreateAcademicTermDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Termin adı boş ola bilməz");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Başlama tarixi boş ola bilməz");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("Bitmə tarixi boş ola bilməz")
                .GreaterThan(x => x.StartDate).WithMessage("Bitmə tarixi başlama tarixindən sonra olmalıdır");
        }
    }
}
