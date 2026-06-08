namespace FTPClient.Models.Models;

public class Result
{
    public bool IsSuccess { get; protected init; }
    public bool IsFailure => !IsSuccess;
    public bool IsValidationFailure => ValidationErrors?.Any() == true;
    public bool IsCancelled { get; protected init; }
    public IEnumerable<object> Errors { get; private set; }
    public IDictionary<string, string[]>? ValidationErrors { get; protected init; }

   

    public static Result Success() => new() { IsSuccess = true };
    public static Result<T> Success<T>(T value) => new(value);

    public static Result ValidationFailure(IDictionary<string, string[]> errors) => 
        new() { IsSuccess = false, ValidationErrors = errors };
    public static Result<T> ValidationFailure<T>(IDictionary<string, string[]> errors) => 
        new(default!) { IsSuccess = false, ValidationErrors = errors };
    
    public static Result Failure(IEnumerable<object> errors) => 
        new() { IsSuccess = false, Errors = errors };
    public static Result<T> Failure<T>(IEnumerable<object> errors) => 
        new(default!) { IsSuccess = false, Errors = errors };

    public static Result Cancelled() => 
        new() { IsSuccess = false, IsCancelled = true };
    public static Result<T> Cancelled<T>() => 
        new(default!) { IsSuccess = false, IsCancelled = true };
}
public class Result<T> : Result
{
    public T Value { get; }
    protected internal Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }
    protected internal Result()
    {
        IsSuccess = false;
        Value = default!;
    }
}