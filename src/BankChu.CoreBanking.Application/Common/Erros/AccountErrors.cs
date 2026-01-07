using BankChu.CoreBanking.Application.Common.Results;

namespace BankChu.CoreBanking.Application.Common.Errors;

public static class AccountErrors
{
    public static readonly Error AlreadyExists =
        new(
            Code: "ACCOUNT_ALREADY_EXISTS",
            Message: "An account with this document already exists.",
            Type: ErrorType.Conflict); // 409

    public static readonly Error InvalidData =
        new(
            Code: "ACCOUNT_INVALID_DATA",
            Message: "Account data is invalid.",
            Type: ErrorType.BusinessValidation); // 422

    public static readonly Error MalformedRequest =
        new(
            Code: "ACCOUNT_BAD_REQUEST",
            Message: "Request payload is invalid.",
            Type: ErrorType.BadRequest); // 400

}