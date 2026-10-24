namespace Polling.Shared;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure
    {
        get
        {
            return !IsSuccess;
        }
    }

    public Error Error { get; }

    public static Result Success()
    {
        return new Result(true, Error.None);
    }

    public static Result Failure(string code, string message)
    {
        return new Result(false, new Error(code, message));
    }
}
