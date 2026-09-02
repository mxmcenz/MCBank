using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MCBank.WebApi.Infrastructure.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var arg = context.ActionArguments.Values.FirstOrDefault();

        if (arg != null)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
            var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;

            if (validator != null)
            {
                var validationContext = new ValidationContext<object>(arg);
                var result = await validator.ValidateAsync(validationContext);

                if (!result.IsValid)
                {
                    var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
                    context.Result = new BadRequestObjectResult(errors);
                    return;
                }
            }
        }

        await next();
    }
}