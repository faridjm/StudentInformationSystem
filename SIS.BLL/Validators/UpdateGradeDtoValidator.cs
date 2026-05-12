using FluentValidation;
using SIS.BLL.Services.DTOs.Grade;

namespace SIS.BLL.Validators
{ 
    public class UpdateGradeDtoValidator : AbstractValidator<UpdateGradeDto>
    {
        public UpdateGradeDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id düzgün deyil");

            RuleFor(x => x.Score)
                .GreaterThanOrEqualTo(0).WithMessage("Bal 0-dan az ola bilməz");
        }
    }
}
