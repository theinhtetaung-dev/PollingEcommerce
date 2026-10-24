namespace Polling.Shared;

public sealed class Error
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }

    public string Code { get; }

    public string Message { get; }

    public override bool Equals(object? obj)
    {
        return obj is Error error && Code == error.Code && Message == error.Message;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Code, Message);
    }
}
