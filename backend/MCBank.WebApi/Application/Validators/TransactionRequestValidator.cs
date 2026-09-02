using FluentValidation;
using MCBank.WebApi.Application.DTOs;

namespace MCBank.WebApi.Application.Validators;

public class TransactionRequestValidator : AbstractValidator<TransactionRequest>
{
    public TransactionRequestValidator()
    {
        RuleFor(x => x.AccountId)
            .GreaterThan(0).WithMessage("ID счета должно быть больше 0");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Сумма должна быть больше 0")
            .PrecisionScale(18, 2, false).WithMessage("Знаки суммы после запятой - 2 символа");
    }
}