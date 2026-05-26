using FluentValidation;
using SIS.BLL.Services.DTOs.AcademicTerm;

namespace SIS.BLL.Validators
{
    public class CreateAcademicTermDtoValidator : AbstractValidator<CreateAcademicTermDto>
    {
        public CreateAcademicTermDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Termin adı boş ola bilməz")
                .MaximumLength(50).WithMessage("Termin adı 50 simvoldan çox ola bilməz");

            RuleFor(x => x.AcademicYearId)
                .GreaterThan(0).WithMessage("Dərs sezonu seçilməlidir");

            RuleFor(x => x.RegistrationStart)
                .NotEmpty().WithMessage("Qeydiyyat başlama tarixi boş ola bilməz");

            RuleFor(x => x.RegistrationEnd)
                .NotEmpty().WithMessage("Qeydiyyat bitmə tarixi boş ola bilməz")
                .GreaterThan(x => x.RegistrationStart)
                .WithMessage("Qeydiyyat bitməsi qeydiyyat başlanğıcından sonra olmalıdır");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Dərslərin başlama tarixi boş ola bilməz")
                .GreaterThanOrEqualTo(x => x.RegistrationEnd)
                .WithMessage("Dərslər qeydiyyat bitdikdən sonra başlamalıdır");

            RuleFor(x => x.DropDeadline)
                .GreaterThan(x => x.StartDate)
                .WithMessage("Drop son tarixi dərslərin başlamasından sonra olmalıdır")
                .LessThan(x => x.EndDate)
                .WithMessage("Drop son tarixi dərslərin bitməsindən əvvəl olmalıdır");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("Dərslərin bitmə tarixi boş ola bilməz")
                .GreaterThan(x => x.StartDate)
                .WithMessage("Dərslərin bitməsi başlamadan sonra olmalıdır");

            RuleFor(x => x.ExamPeriodStart)
                .GreaterThanOrEqualTo(x => x.EndDate)
                .WithMessage("İmtahan dövrü dərslərin bitməsindən sonra başlamalıdır");

            RuleFor(x => x.ExamPeriodEnd)
                .GreaterThan(x => x.ExamPeriodStart)
                .WithMessage("İmtahan dövrü bitməsi başlamasından sonra olmalıdır");

            RuleFor(x => x.GradeSubmissionDeadline)
                .GreaterThanOrEqualTo(x => x.ExamPeriodEnd)
                .WithMessage("Qiymət təsdiqi son tarixi imtahan dövrünün bitməsindən sonra olmalıdır");
        }
    }
}
