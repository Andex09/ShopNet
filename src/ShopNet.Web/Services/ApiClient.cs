using System.Net;
using System.Net.Http.Json;

namespace ShopNet.Web.Services;

/// <summary>Result of an API call: the status code and, on success, the body.</summary>
public sealed record ApiResult<T>(HttpStatusCode StatusCode, T? Value)
{
    public bool IsSuccess => (int)StatusCode is >= 200 and <= 299;
}

/// <summary>Result of an API call without a body.</summary>
public sealed record ApiResult(HttpStatusCode StatusCode)
{
    public bool IsSuccess => (int)StatusCode is >= 200 and <= 299;
}

/// <summary>
/// Thin wrapper over HttpClient. Network failures become 503 results instead of exceptions,
/// so components only branch on status codes.
/// </summary>
public sealed class ApiClient(HttpClient http)
{
    public async Task<ApiResult<T>> GetAsync<T>(string uri, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await http.GetAsync(uri, cancellationToken);
            return await ReadAsync<T>(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return new(HttpStatusCode.ServiceUnavailable, default);
        }
    }

    public async Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(
        string uri, TRequest body, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await http.PostAsJsonAsync(uri, body, cancellationToken);
            return await ReadAsync<TResponse>(response, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return new(HttpStatusCode.ServiceUnavailable, default);
        }
    }

    public async Task<ApiResult> PutAsync<TRequest>(string uri, TRequest body, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await http.PutAsJsonAsync(uri, body, cancellationToken);
            return new(response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return new(HttpStatusCode.ServiceUnavailable);
        }
    }

    public async Task<ApiResult> DeleteAsync(string uri, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await http.DeleteAsync(uri, cancellationToken);
            return new(response.StatusCode);
        }
        catch (HttpRequestException)
        {
            return new(HttpStatusCode.ServiceUnavailable);
        }
    }

    private static async Task<ApiResult<T>> ReadAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken) =>
        response.IsSuccessStatusCode
            ? new(response.StatusCode, await response.Content.ReadFromJsonAsync<T>(cancellationToken))
            : new(response.StatusCode, default);
}
