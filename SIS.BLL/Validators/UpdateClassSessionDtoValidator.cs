using FluentValidation;
using SIS.BLL.Services.DTOs.ClassSession;

namespace SIS.BLL.Validators
{
    public class UpdateClassSessionDtoValidator : AbstractValidator<UpdateClassSessionDto>
    {
        public UpdateClassSessionDtoValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id düzgün deyil");

            RuleFor(x => x.Date)
                .NotEqual(default(DateOnly)).WithMessage("Tarix boş ola bilməz");

            RuleFor(x => x.StartTime)
                .NotEqual(default(TimeOnly)).WithMessage("Başlama saatı boş ola bilməz");

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime).WithMessage("Bitmə saatı başlama saatından sonra olmalıdır");

            RuleFor(x => x.Room).MaximumLength(50);
            RuleFor(x => x.Topic).MaximumLength(200);
            RuleFor(x => x.Notes).MaximumLength(2000);
        }
    }

    public class CancelClassSessionDtoValidator : AbstractValidator<CancelClassSessionDto>
    {
        public CancelClassSessionDtoValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0).WithMessage("Id düzgün deyil");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Ləğv səbəbi boş ola bilməz")
                .MaximumLength(500).WithMessage("Səbəb 500 simvoldan çox ola bilməz");
        }
    }

    public class CreateMakeupSessionDtoValidator : AbstractValidator<CreateMakeupSessionDto>
    {
        public CreateMakeupSessionDtoValidator()
        {
            RuleFor(x => x.OriginalSessionId)
                .GreaterThan(0).WithMessage("Əvəz olunan sessiya seçilməlidir");

            RuleFor(x => x.Date)
                .NotEqual(default(DateOnly)).WithMessage("Tarix boş ola bilməz");

            RuleFor(x => x.StartTime)
                .NotEqual(default(TimeOnly)).WithMessage("Başlama saatı boş ola bilməz");

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime).WithMessage("Bitmə saatı başlama saatından sonra olmalıdır");

            RuleFor(x => x.Room).MaximumLength(50);
            RuleFor(x => x.Topic).MaximumLength(200);
        }
    }
}
