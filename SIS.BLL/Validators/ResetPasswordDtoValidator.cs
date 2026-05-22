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
                .MinimumLength(6).WithMessage("Şifrə minimum 6 simvol olmalıdır")
                .Matches("[A-Z]").WithMessage("Şifrədə ən az 1 böyük hərf olmalıdır")
                .Matches("[a-z]").WithMessage("Şifrədə ən az 1 kiçik hərf olmalıdır")
                .Matches("[0-9]").WithMessage("Şifrədə ən az 1 rəqəm olmalıdır");

            RuleFor(x => x.ConfirmNewPassword)
                .Equal(x => x.NewPassword).WithMessage("Şifrələr uyğun gəlmir");
        }
    }

}
