using FluentValidation;
using SIS.BLL.Services.DTOs.Teacher;

namespace SIS.BLL.Validators
{
    public class UpdateTeacherDtoValidator : AbstractValidator<UpdateTeacherDto>
    {
        public UpdateTeacherDtoValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        }
    }
}
