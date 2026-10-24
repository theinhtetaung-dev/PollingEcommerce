namespace Polling.Shared;

public record Error(string Code, string Message) { public static readonly Error None = new(string.Empty, string.Empty); }

public class Result
{
    protected Result(bool isSuccess, Error error) { IsSuccess = isSuccess; Error = error; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    public static Result Success() => new(true, Error.None);
    public static Result Failure(string code, string message) => new(false, new(code, message));
}

public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, Error error) : base(isSuccess, error) => Value = value;
    public T? Value { get; }
    public static Result<T> Success(T value) => new(true, value, Error.None);
    public new static Result<T> Failure(string code, string message) => new(false, default, new(code, message));
}
