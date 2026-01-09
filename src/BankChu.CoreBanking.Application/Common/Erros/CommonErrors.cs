using BankChu.CoreBanking.Application.Common.Results;

namespace BankChu.CoreBanking.Application.Common.Errors;

public static class CommonErrors
{
    public static Error BadRequest(string code, string message) =>
        new(
            code: code,
            message: message,
            type: ErrorType.BadRequest);

    public static Error Validation(string code, string message) =>
        new(
            code: code,
            message: message,
            type: ErrorType.BusinessValidation);

    public static Error Conflict(string code, string message) =>
        new(
            code: code,
            message: message,
            type: ErrorType.Conflict);

    public static Error NotFound(string code, string message) =>
        new(
            code: code,
            message: message,
            type: ErrorType.NotFound);

    public static Error Unauthorized(string code, string message) =>
        new(
            code: code,
            message: message,
            type: ErrorType.Unauthorized);

    public static Error Forbidden(string code, string message) =>
        new(
            code: code,
            message: message,
            type: ErrorType.Forbidden);

    public static Error Internal(string code, string message) =>
        new(
            code: code,
            message: message,
            type: ErrorType.Internal);
}
