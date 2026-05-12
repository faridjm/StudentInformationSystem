using FluentValidation;
using SIS.BLL.Services.DTOs.AcademicTerm;

namespace SIS.BLL.Validators
{ 
    public class UpdateAcademicTermDtoValidator : AbstractValidator<UpdateAcademicTermDto>
    {
        public UpdateAcademicTermDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id düzgün deyil");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Termin adı boş ola bilməz");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate).WithMessage("Bitmə tarixi başlama tarixindən sonra olmalıdır");
        }
    }
}
