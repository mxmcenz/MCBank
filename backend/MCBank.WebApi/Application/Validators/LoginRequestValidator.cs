using FluentValidation;
using MCBank.WebApi.Application.DTOs;

namespace MCBank.WebApi.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Имя пользователя не может быть пустым")
            .MinimumLength(3).WithMessage("Минимальная длина имени - 3 символа");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль не может быть пустым")
            .MinimumLength(5).WithMessage("Пароль должен быть не менее 5 символов");
    }
}