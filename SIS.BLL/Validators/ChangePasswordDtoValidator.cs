using FluentValidation;
using SIS.BLL.Services.DTOs.Auth;

namespace SIS.BLL.Validators
{
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Cari şifrə boş ola bilməz");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Yeni şifrə boş ola bilməz")
                .MinimumLength(8).WithMessage("Şifrə minimum 8 simvol olmalıdır")
                .Matches("[A-Z]").WithMessage("Şifrədə ən az 1 böyük hərf olmalıdır")
                .Matches("[a-z]").WithMessage("Şifrədə ən az 1 kiçik hərf olmalıdır")
                .Matches("[0-9]").WithMessage("Şifrədə ən az 1 rəqəm olmalıdır");

            RuleFor(x => x.ConfirmNewPassword)
                .Equal(x => x.NewPassword).WithMessage("Şifrələr uyğun gəlmir");
        }
    }

}
