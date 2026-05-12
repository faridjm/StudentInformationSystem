using FluentValidation;
using SIS.BLL.Services.DTOs.Student;

namespace SIS.BLL.Validators
{
    public class CreateStudentDtoValidator : AbstractValidator<CreateStudentDto>
    {
        public CreateStudentDtoValidator()
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

            RuleFor(x => x.EnrollmentYear)
                .GreaterThanOrEqualTo(2000).WithMessage("Qeydiyyat ili 2000-dən az ola bilməz")
                .LessThanOrEqualTo(DateTime.Now.Year + 1).WithMessage("Qeydiyyat ili düzgün deyil");

            RuleFor(x => x.AcademicProgramId)
                .GreaterThan(0).WithMessage("Proqram seçilməlidir");
        }
    }

}
