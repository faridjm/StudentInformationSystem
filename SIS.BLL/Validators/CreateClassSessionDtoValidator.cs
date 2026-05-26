using FluentValidation;
using SIS.BLL.Services.DTOs.ClassSession;

namespace SIS.BLL.Validators
{
    public class CreateClassSessionDtoValidator : AbstractValidator<CreateClassSessionDto>
    {
        public CreateClassSessionDtoValidator()
        {
            RuleFor(x => x.CourseOfferingId)
                .GreaterThan(0).WithMessage("Kurs təklifi seçilməlidir");

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
}
