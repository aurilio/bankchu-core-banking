namespace BankChu.CoreBanking.Application.Common.Results;

public enum ErrorType
{
    BusinessValidation, // 422
    BadRequest,         // 400
    Conflict,           // 409
    NotFound,           // 404
    Unauthorized,       // 401
    Forbidden,          // 403
    Internal            // 500 (fallback técnico)
}