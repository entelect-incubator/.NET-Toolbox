namespace PaymentSaga.Domain.Enums;

public enum PaymentStatus
{
    Initiated,
    Validating,
    AwaitingApproval,
    Approved,
    Processing,
    Settling,
    Settled,
    Rejected,
    Failed,
    Cancelled
}
