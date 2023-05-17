// learn more at https://aka.ms/dab

namespace DabHelpers;

public interface IRestHelper<T> where T : new()
{
    Task<T?> GetOneAsync(T model);
    Task<(IEnumerable<T> Items, string ContinuationUrl)> GetManyAsync(string? select = null, string? filter = null, string? orderby = null, int? first = null, int? after = null);

    IAsyncEnumerable<(T Input, T? Result, Exception? Error)> InsertAsync(IEnumerable<T> models);
    Task<T> InsertAsync(T model);

    IAsyncEnumerable<(T Input, T? Result, Exception? Error)> UpdateAsync(IEnumerable<T> models);
    Task<T> UpdateAsync(T model);

    IAsyncEnumerable<(T Input, T? Result, Exception? Error)> UpsertAsync(IEnumerable<T> models);
    Task<T> UpsertAsync(T model);

    IAsyncEnumerable<(T Input, Exception? Error)> DeleteAsync(IEnumerable<T> models);
    Task DeleteAsync(T model);
}