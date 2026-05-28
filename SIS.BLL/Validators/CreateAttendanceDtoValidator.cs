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
                .NotEmpty().WithMessage("Sessiya tarixi boş ola bilməz");
            // Qeyd: "Gələcək tarix qadağan" qaydası AttendanceService-də yoxlanır,
            // çünki admin/superadmin bypass etmək imkanı olmalıdır.
        }
    }
}
