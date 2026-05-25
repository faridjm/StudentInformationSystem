using FluentValidation;
using SIS.BLL.Services.DTOs.Attendance;

namespace SIS.BLL.Validators
{

    public class CreateAttendanceDtoValidator : AbstractValidator<CreateAttendanceDto>
    {
        public CreateAttendanceDtoValidator()
        {
            RuleFor(x => x.EnrollmentId)
                .GreaterThan(0).WithMessage("Qeydiyyat seçilməlidir");

            RuleFor(x => x.SessionDate)
                .NotEmpty().WithMessage("Sessiya tarixi boş ola bilməz")
                .Must(date => date.Date <= DateTime.Now.Date)
                .WithMessage("Sessiya tarixi gələcəkdə ola bilməz");
        }
    }
}
