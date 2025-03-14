using Orders.Service.Helpers;
using Orders.Shared.Contracts;
using RestSharp;

namespace Orders.Service.Clients;

public class ChannelEngineClient : IChannelEngineClient
{
    private readonly RestClient _restClient;
    private readonly ILogger<ChannelEngineClient> _logger;
    private readonly string _apiKey;

    public ChannelEngineClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ChannelEngineClient> logger)
    {
        _logger = logger;
        var channelEngineApiBaseUrl = configuration.RetrieveConfigurationValue("ChannelEngineApiBaseUrl");
        _apiKey = configuration.RetrieveConfigurationValue("ChannelEngineApiKey");
        
        var httpClientInstance = httpClientFactory.CreateClient(nameof(ChannelEngineClient));
        _restClient = new RestClient(httpClientInstance, new RestClientOptions($"{channelEngineApiBaseUrl}"));
    }

    public Task<Result<T>> ExecuteAsync<T>(RestRequest request, CancellationToken cancellationToken) where T : new()
    {
        throw new NotImplementedException();
    }
}