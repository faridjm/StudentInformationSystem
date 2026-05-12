using FluentValidation;
using SIS.BLL.Services.DTOs.Department;

namespace SIS.BLL.Validators
{
    public class UpdateDepartmentDtoValidator : AbstractValidator<UpdateDepartmentDto>
    {
        public UpdateDepartmentDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id düzgün deyil");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ad boş ola bilməz");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Kod boş ola bilməz")
                .MaximumLength(10).WithMessage("Kod maksimum 10 simvol ola bilər");
        }
    }
}
