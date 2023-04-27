// learn more at https://aka.ms/dab

namespace DabHelpers;

public interface IRestHelper<T> where T : new()
{
    Task<RestResult<T?>> GetOneAsync(params (string Name, object Value)[] keys);
    Task<(RestResult<IEnumerable<T>> result, string ContinuationUrl)> GetManyAsync(string? select = null, string? filter = null, string? orderby = null, int? first = null, int? after = null);

    IAsyncEnumerable<(T model, RestResult<T> Result)> InsertAsync(IEnumerable<T> models);
    Task<RestResult<T>> InsertAsync(T model);

    IAsyncEnumerable<(T Model, RestResult<T> Result)> UpdateAsync(IEnumerable<T> models);
    Task<RestResult<T>> UpdateAsync(T model);

    IAsyncEnumerable<(T Model, RestResult<T> Result)> UpsertAsync(IEnumerable<T> models);
    Task<RestResult<T>> UpsertAsync(T model);

    Task DeleteAsync(IEnumerable<T> models);
    Task<RestResult<bool>> DeleteAsync(T model);
}
