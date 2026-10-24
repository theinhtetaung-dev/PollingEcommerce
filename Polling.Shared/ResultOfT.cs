namespace Polling.Shared;

public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, Error error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, Error.None);
    }

    public new static Result<T> Failure(string code, string message)
    {
        return new Result<T>(false, default, new Error(code, message));
    }
}
