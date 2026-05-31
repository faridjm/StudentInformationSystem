using FluentValidation;
using SIS.BLL.Services.DTOs.Student;

namespace SIS.BLL.Validators
{
    public class UpdateStudentDtoValidator : AbstractValidator<UpdateStudentDto>
    {
        public UpdateStudentDtoValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        }
    }
}
