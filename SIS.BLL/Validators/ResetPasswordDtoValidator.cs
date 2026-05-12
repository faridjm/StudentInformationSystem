using FluentValidation;
using SIS.BLL.Services.DTOs.Auth;

namespace SIS.BLL.Validators
{
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email boş ola bilməz")
                .EmailAddress().WithMessage("Email düzgün deyil");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token boş ola bilməz");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Yeni şifrə boş ola bilməz")
                .MinimumLength(8).WithMessage("Şifrə minimum 8 simvol olmalıdır")
                .Matches("[A-Z]").WithMessage("Böyük hərf lazımdır")
                .Matches("[a-z]").WithMessage("Kiçik hərf lazımdır")
                .Matches("[0-9]").WithMessage("Rəqəm lazımdır");

            RuleFor(x => x.ConfirmNewPassword)
                .Equal(x => x.NewPassword).WithMessage("Şifrələr uyğun gəlmir");
        }
    }

}
