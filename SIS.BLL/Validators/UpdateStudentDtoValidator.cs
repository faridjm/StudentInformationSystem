using FluentValidation;
using SIS.BLL.Services.DTOs.Student;

namespace SIS.BLL.Validators
{
    public class UpdateStudentDtoValidator : AbstractValidator<UpdateStudentDto>
    {
        public UpdateStudentDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id düzgün deyil");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Ad boş ola bilməz")
                .MinimumLength(3).WithMessage("Ad minimum 3 simvol olmalıdır");

            RuleFor(x => x.AcademicProgramId)
                .GreaterThan(0).WithMessage("Proqram seçilməlidir");
        }
    }

}
