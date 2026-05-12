using FluentValidation;
using SIS.BLL.Services.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace SIS.BLL.Validators
{
    public class LoginDtoValidator : AbstractValidator<LoginDto>
    {
        public LoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email boş ola bilməz")
                .EmailAddress().WithMessage("Email düzgün deyil");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifrə boş ola bilməz")
                .MinimumLength(8).WithMessage("Şifrə minimum 8 simvol olmalıdır");
        }
    }

}
