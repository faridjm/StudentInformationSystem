using FluentValidation;
using SIS.BLL.Services.DTOs.Announcement;

namespace SIS.BLL.Validators
{
    public class CreateAnnouncementDtoValidator : AbstractValidator<CreateAnnouncementDto>
    {
        public CreateAnnouncementDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Başlıq boş ola bilməz")
                .MinimumLength(3).WithMessage("Başlıq minimum 3 simvol olmalıdır")
                .MaximumLength(200).WithMessage("Başlıq maksimum 200 simvol ola bilər");

            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Məzmun boş ola bilməz")
                .MinimumLength(10).WithMessage("Məzmun minimum 10 simvol olmalıdır");
        }
    }
}
