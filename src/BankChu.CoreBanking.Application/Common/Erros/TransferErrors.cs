using BankChu.CoreBanking.Application.Common.Results;

namespace BankChu.CoreBanking.Application.Common.Erros;

public static class TransferErrors
{
    public static readonly Error InvalidAmount =
        new(
            code: "TRANSFER_INVALID_AMOUNT",
            message: "Transfer amount must be greater than zero.",
            type: ErrorType.BadRequest);

    public static readonly Error SameAccount =
        new(
            code: "TRANSFER_SAME_ACCOUNT",
            message: "Source and destination accounts must be different.",
            type: ErrorType.BadRequest);

    public static readonly Error NotBusinessDay =
        new(
            code: "TRANSFER_NOT_BUSINESS_DAY",
            message: "Transfers are allowed only on business days.",
            type: ErrorType.BusinessValidation);

    public static readonly Error SourceAccountNotFound =
        new (
            code: "TRANSFER_SOURCE_NOT_FOUND",
            message: "Source account was not found.",
            type: ErrorType.NotFound);

    public static readonly Error DestinationAccountNotFound =
        new(
            code: "TRANSFER_DESTINATION_NOT_FOUND",
            message: "Destination account was not found.",
            type: ErrorType.NotFound);

    public static readonly Error SourceAccountInactive =
        new(
            code: "TRANSFER_SOURCE_INACTIVE",
            message: "Source account is inactive.",
            type: ErrorType.BusinessValidation);
   
    public static readonly Error DestinationAccountInactive =
        new (
            code: "TRANSFER_DESTINATION_INACTIVE",
            message: "Destination account is inactive.",
            type: ErrorType.BusinessValidation);

    public static readonly Error InsufficientBalance =
        new(
            code: "TRANSFER_INSUFFICIENT_BALANCE",
            message: "Source account has insufficient balance.",
            type: ErrorType.BusinessValidation);

    public static readonly Error DuplicateRequestInProgress =
        new(
            code: "TRANSFER_IDEMPOTENCY_IN_PROGRESS",
            message: "Another request with the same idempotency key is being processed.",
            type: ErrorType.Conflict);

    public static readonly Error InvalidData =
        new(
            code: "TRANSFER_INVALID_DATA",
            message: "Transfer request contains invalid data.",
            type: ErrorType.BadRequest);

    public static readonly Error ValidationFailed =
        new(
            code: "TRANSFER_VALIDATION_FAILED",
            message: "Transfer request validation failed.",
            type: ErrorType.BadRequest);
}