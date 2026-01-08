using BankChu.CoreBanking.Application.Common.Results;

namespace BankChu.CoreBanking.Application.Common.Erros;

public static class TransferErrors
{
    public static readonly Error InvalidAmount =
        new(
            Code: "TRANSFER_INVALID_AMOUNT",
            Message: "Transfer amount must be greater than zero.",
            Type: ErrorType.BadRequest);

    public static readonly Error SameAccount =
        new(
            Code: "TRANSFER_SAME_ACCOUNT",
            Message: "Source and destination accounts must be different.",
            Type: ErrorType.BadRequest);

    public static readonly Error NotBusinessDay =
        new(
            Code: "TRANSFER_NOT_BUSINESS_DAY",
            Message: "Transfers are allowed only on business days.",
            Type: ErrorType.BusinessValidation);

    public static readonly Error SourceAccountNotFound =
        new (
            Code: "TRANSFER_SOURCE_NOT_FOUND",
            Message: "Source account was not found.",
            Type: ErrorType.NotFound);

    public static readonly Error DestinationAccountNotFound =
        new(
            Code: "TRANSFER_DESTINATION_NOT_FOUND",
            Message: "Destination account was not found.",
            Type: ErrorType.NotFound);

    public static readonly Error SourceAccountInactive =
        new(
            Code: "TRANSFER_SOURCE_INACTIVE",
            Message: "Source account is inactive.",
            Type: ErrorType.BusinessValidation);
   
    public static readonly Error DestinationAccountInactive =
        new (
            Code: "TRANSFER_DESTINATION_INACTIVE",
            Message: "Destination account is inactive.",
            Type: ErrorType.BusinessValidation);

    public static readonly Error InsufficientBalance =
        new(
            Code: "TRANSFER_INSUFFICIENT_BALANCE",
            Message: "Source account has insufficient balance.",
            Type: ErrorType.BusinessValidation);

    public static readonly Error DuplicateRequestInProgress =
        new(
            Code: "TRANSFER_IDEMPOTENCY_IN_PROGRESS",
            Message: "Another request with the same idempotency key is being processed.",
            Type: ErrorType.Conflict);
}