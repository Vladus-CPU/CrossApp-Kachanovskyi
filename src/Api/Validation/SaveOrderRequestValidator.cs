using Api.Contracts;
using FluentValidation;

namespace Api.Validation;

public sealed class SaveOrderRequestValidator : AbstractValidator<SaveOrderRequest>
{
    public SaveOrderRequestValidator()
    {
        RuleFor(request => request.CustomerId).NotEmpty().WithMessage("Ідентифікатор клієнта обов'язковий.").MaximumLength(100).WithMessage("Ідентифікатор клієнта до 100 символів.");

        RuleFor(request => request.Lines).NotEmpty().WithMessage("Замовлення повинно містити хоча б один рядок.");

        RuleForEach(request => request.Lines).NotNull().WithMessage("Рядок замовлення не може бути порожнім.").SetValidator(new SaveOrderLineRequestValidator());
    }
}

public sealed class SaveOrderLineRequestValidator : AbstractValidator<SaveOrderLineRequest>
{
    public SaveOrderLineRequestValidator()
    {
        RuleFor(line => line.ProductId).NotEmpty().WithMessage("Ідентифікатор товару обов'язковий.").MaximumLength(100).WithMessage("Ідентифікатор товару до 100 символів.");

        RuleFor(line => line.Name).NotEmpty().WithMessage("Назва товару обов'язкова.").MaximumLength(200).WithMessage("Назва товару до 200 символів.");

        RuleFor(line => line.Price).NotNull().WithMessage("Ціна обов'язкова.").GreaterThanOrEqualTo(0m).WithMessage("Ціна не може бути від'ємною.").LessThanOrEqualTo(1_000_000_000m).WithMessage("Занадто велика ціна.");

        RuleFor(line => line.Quantity).NotNull().WithMessage("Кількість обов'язкова.").GreaterThan(0).WithMessage("Кількість має бути більшою за нуль.");
    }
}