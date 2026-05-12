using FluentValidation;
using SIS.BLL.Services.DTOs.Grade;

namespace SIS.BLL.Validators
{   
    public class CreateGradeDtoValidator : AbstractValidator<CreateGradeDto>
    {
        public CreateGradeDtoValidator()
        {
            RuleFor(x => x.EnrollmentId)
                .GreaterThan(0).WithMessage("Qeydiyyat seçilməlidir");

            RuleFor(x => x.Score)
                .GreaterThanOrEqualTo(0).WithMessage("Bal 0-dan az ola bilməz");

            RuleFor(x => x.MaxScore)
                .GreaterThan(0).WithMessage("Maksimum bal 0-dan böyük olmalıdır");

            RuleFor(x => x.Score)
                .LessThanOrEqualTo(x => x.MaxScore).WithMessage("Bal maksimum baldan çox ola bilməz");
        }
    }
}
