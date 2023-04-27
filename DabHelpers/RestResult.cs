// learn more at https://aka.ms/dab

namespace DabHelpers;

public class RestResult<T>
{
    public RestResult(T result) => Result = result;
    public RestResult(Exception error) => Errors.Add(error);

    public T Result { get; set; } = default!;
    public List<Exception> Errors { get; } = new();
    public bool Success => !Errors.Any();

    public static implicit operator RestResult<T>(T result) => new(result);
    public static implicit operator RestResult<T>(Exception error) => new(error);
}
