namespace BankChu.CoreBanking.Application.Common.Results;

public sealed class Error
{
    public string Code { get; }

    public string Message { get; }

    public ErrorType Type { get; }

    public IReadOnlyCollection<string>? Details { get; }

    public Error(
        string code,
        string message,
        ErrorType type,
        IReadOnlyCollection<string>? details = null)
    {
        Code = code;
        Message = message;
        Type = type;
        Details = details;
    }

    public Error WithDetails(params string[] details)
        => new(Code, Message, Type, details);
}