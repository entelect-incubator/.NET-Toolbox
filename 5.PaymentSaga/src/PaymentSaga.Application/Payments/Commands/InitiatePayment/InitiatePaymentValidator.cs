namespace PaymentSaga.Application.Payments.Commands.InitiatePayment;

using FluentValidation;

public sealed class InitiatePaymentValidator : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentValidator()
    {
        RuleFor(x => x.PayerId)
            .NotEmpty().WithMessage("PayerId is required.");

        RuleFor(x => x.PayeeId)
            .NotEmpty().WithMessage("PayeeId is required.")
            .NotEqual(x => x.PayerId).WithMessage("PayeeId cannot be the same as PayerId.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a valid 3-letter ISO code.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
