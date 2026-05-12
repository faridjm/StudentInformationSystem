using FluentValidation;
using SIS.BLL.Services.DTOs.Department;

namespace SIS.BLL.Validators
{ 
    public class CreateDepartmentDtoValidator : AbstractValidator<CreateDepartmentDto>
    {
        public CreateDepartmentDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Ad boş ola bilməz");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Kod boş ola bilməz")
                .MaximumLength(10).WithMessage("Kod maksimum 10 simvol ola bilər");
        }
    }
}
