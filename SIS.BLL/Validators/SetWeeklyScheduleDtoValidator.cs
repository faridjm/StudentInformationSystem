using FluentValidation;
using SIS.BLL.Services.DTOs.WeeklySchedule;

namespace SIS.BLL.Validators
{
    public class SetWeeklyScheduleDtoValidator : AbstractValidator<SetWeeklyScheduleDto>
    {
        public SetWeeklyScheduleDtoValidator()
        {
            RuleFor(x => x.CourseOfferingId)
                .GreaterThan(0).WithMessage("Kurs təklifi seçilməlidir");

            RuleFor(x => x.Slots)
                .NotNull().WithMessage("Cədvəl boş ola bilməz")
                .Must(s => s.Count > 0).WithMessage("Ən azı bir slot əlavə edin")
                .Must(s => s.Count <= 7).WithMessage("Həftədə 7 gündən çox slot ola bilməz");

            RuleForEach(x => x.Slots).SetValidator(new WeeklySlotDtoValidator());

            // Eyni günün eyni saatla təkrarı olmamalıdır
            RuleFor(x => x.Slots).Must(slots =>
                slots.GroupBy(s => new { s.DayOfWeek, s.StartTime }).All(g => g.Count() == 1))
                .WithMessage("Eyni günün eyni başlama saatı təkrar edilə bilməz");
        }
    }

    public class WeeklySlotDtoValidator : AbstractValidator<WeeklySlotDto>
    {
        public WeeklySlotDtoValidator()
        {
            RuleFor(x => x.DayOfWeek)
                .IsInEnum().WithMessage("Həftə günü düzgün deyil");

            RuleFor(x => x.StartTime)
                .NotEqual(default(TimeOnly)).WithMessage("Başlama saatı boş ola bilməz");

            RuleFor(x => x.EndTime)
                .NotEqual(default(TimeOnly)).WithMessage("Bitmə saatı boş ola bilməz")
                .GreaterThan(x => x.StartTime).WithMessage("Bitmə saatı başlama saatından sonra olmalıdır");

            RuleFor(x => x.Room)
                .MaximumLength(50).WithMessage("Otaq adı 50 simvoldan çox ola bilməz");
        }
    }
}
