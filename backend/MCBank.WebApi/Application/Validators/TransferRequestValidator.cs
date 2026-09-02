using FluentValidation;
using MCBank.WebApi.Application.DTOs;

namespace MCBank.WebApi.Application.Validators;

public class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        RuleFor(x => x.FromAccountId)
            .GreaterThan(0).WithMessage("ID счета отправителя должно быть больше 0")
            .NotEqual(x => x.ToAccountId).WithMessage("Нельзя перевести деньги на тот же счет");
        
        RuleFor(x => x.ToAccountId)
            .GreaterThan(0).WithMessage("ID счета получателя должно быть больше 0");
        
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Сумма должна быть больше 0")
            .PrecisionScale(18, 2, false).WithMessage("Знаки суммы после запятой - 2 символа");
    }
}