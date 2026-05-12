using FluentValidation;
using SIS.BLL.Services.DTOs.Teacher;

namespace SIS.BLL.Validators
{
    public class UpdateTeacherDtoValidator : AbstractValidator<UpdateTeacherDto>
    {
        public UpdateTeacherDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id düzgün deyil");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Ad boş ola bilməz")
                .MinimumLength(3).WithMessage("Ad minimum 3 simvol olmalıdır");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Titul boş ola bilməz");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Departament seçilməlidir");
        }
    }

}
