using FluentValidation;
using SIS.BLL.Services.DTOs.Teacher;

namespace SIS.BLL.Validators
{
    public class CreateTeacherDtoValidator : AbstractValidator<CreateTeacherDto>
    {
        public CreateTeacherDtoValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Ad boş ola bilməz")
                .MinimumLength(3).WithMessage("Ad minimum 3 simvol olmalıdır");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email boş ola bilməz")
                .EmailAddress().WithMessage("Email düzgün deyil");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifrə boş ola bilməz")
                .MinimumLength(8).WithMessage("Şifrə minimum 8 simvol olmalıdır")
                .Matches("[A-Z]").WithMessage("Böyük hərf lazımdır")
                .Matches("[a-z]").WithMessage("Kiçik hərf lazımdır")
                .Matches("[0-9]").WithMessage("Rəqəm lazımdır");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Titul boş ola bilməz");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0).WithMessage("Departament seçilməlidir");

            RuleFor(x => x.HireDate)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("İşə başlama tarixi düzgün deyil");
        }
    }

}
