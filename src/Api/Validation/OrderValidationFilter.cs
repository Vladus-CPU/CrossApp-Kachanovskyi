using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class OrderValidationFilter(IValidator<SaveOrderRequest> validator) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<SaveOrderRequest>().FirstOrDefault();

        if (request is null)
        {
            var errors = new Dictionary<string, string[]>();
            errors["Body"] = ["Тіло запиту обов'язкове."];

            return TypedResults.ValidationProblem(errors);
        }

        var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);

        if (!result.IsValid)
        {
            var errors = result.Errors.GroupBy(error => error.PropertyName).ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

            return TypedResults.ValidationProblem(errors);
        }

        return await next(context);
    }
}