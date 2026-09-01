namespace MCBank.WebApi.Core.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public ErrorType ErrorType { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, string error, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, string.Empty, ErrorType.None);
    public static Result Failure(string error, ErrorType errorType = ErrorType.Failure) => new(false, error, errorType);
}

public sealed class Result<T> : Result
{
    public T Value { get; }

    private Result(T value, bool isSuccess, string error, ErrorType errorType) : base(isSuccess, error, errorType)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true, string.Empty, ErrorType.None);
    public new static Result<T> Failure(string error, ErrorType errorType = ErrorType.Failure)
        => new(default!, false, error, errorType);
}