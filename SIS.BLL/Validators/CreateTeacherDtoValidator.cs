using FluentValidation;
using SIS.BLL.Services.DTOs.Teacher;

namespace SIS.BLL.Validators
{
    public class CreateTeacherDtoValidator : AbstractValidator<CreateTeacherDto>
    {
        public CreateTeacherDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Ad Soyad boş ola bilməz")
                .MinimumLength(3).WithMessage("Ad Soyad minimum 3 simvol olmalıdır");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email boş ola bilməz")
                .EmailAddress().WithMessage("Email düzgün deyil");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifrə boş ola bilməz")
                .MinimumLength(6).WithMessage("Şifrə minimum 6 simvol olmalıdır")
                .Matches("[A-Z]").WithMessage("Şifrədə ən az 1 böyük hərf olmalıdır")
                .Matches("[0-9]").WithMessage("Şifrədə ən az 1 rəqəm olmalıdır");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Titul boş ola bilməz");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Departament seçilməlidir");
        }
    }

}
