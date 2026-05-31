using FluentValidation;
using SIS.BLL.Services.DTOs.Student;

namespace SIS.BLL.Validators
{
    public class CreateStudentDtoValidator : AbstractValidator<CreateStudentDto>
    {
        public CreateStudentDtoValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
            RuleFor(x => x.EnrollmentYear).InclusiveBetween(2000, DateTime.Now.Year + 1);
        }
    }
}
