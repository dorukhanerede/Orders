using Orders.Shared.Contracts;
using RestSharp;

namespace Orders.Service.Clients;

public interface IChannelEngineClient
{
    Task<Result<T>> ExecuteAsync<T>(RestRequest request, CancellationToken cancellationToken) where T : new();
    Task<Result> ExecuteAsync(RestRequest request, CancellationToken cancellationToken);
}