using BankChu.CoreBanking.Application.Common.Results;

namespace BankChu.CoreBanking.Application.Common.Errors;

public static class AccountErrors
{
    public static readonly Error AlreadyExists =
        new(
            code: "ACCOUNT_ALREADY_EXISTS",
            message: "An account with this document already exists.",
            type: ErrorType.Conflict); // 409

    public static readonly Error InvalidData =
        new(
            code: "ACCOUNT_INVALID_DATA",
            message: "Account data is invalid.",
            type: ErrorType.BusinessValidation); // 422

    public static readonly Error MalformedRequest =
        new(
            code: "ACCOUNT_BAD_REQUEST",
            message: "Request payload is invalid.",
            type: ErrorType.BadRequest); // 400

    public static Error NotFound(Guid accountId) =>
        new(
            code: "ACCOUNT_NOT_FOUND",
            message: $"Account with id '{accountId}' was not found.",
            type: ErrorType.NotFound); // 404
}