// learn more at https://aka.ms/dab

namespace DabHelpers;

public class RestResult<T>
{
    public RestResult(T result) => Result = result;
    public RestResult(Exception error) => Error = error;

    public T? Result { get; } = default!;
    public Exception? Error { get; }
    public bool Success => Error is not null;

    public static implicit operator T(RestResult<T> result) => result.Result ?? default!;
    public static implicit operator RestResult<T>(T result) => new(result);
    public static implicit operator RestResult<T>(Exception error) => new(error);
}
