using System.Net;
using Newtonsoft.Json;
using Orders.Service.Helpers;
using Orders.Shared.Contracts;
using Polly;
using Polly.RateLimit;
using Polly.Wrap;
using RestSharp;

namespace Orders.Service.Clients;

public class ChannelEngineClient : IChannelEngineClient
{
    private readonly RestClient _restClient;
    private readonly ILogger<ChannelEngineClient> _logger;
    private readonly string _apiKey;
    private readonly AsyncPolicyWrap<RestResponse> _policy;

    public ChannelEngineClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ChannelEngineClient> logger)
    {
        _logger = logger;
        var channelEngineApiBaseUrl = configuration.RetrieveConfigurationValue("ChannelEngine:ChannelEngineApiBaseUrl");
        _apiKey = configuration.RetrieveConfigurationValue("ChannelEngine:ChannelEngineApiKey");
        
        var httpClientInstance = httpClientFactory.CreateClient(nameof(ChannelEngineClient));
        _restClient = new RestClient(httpClientInstance, new RestClientOptions($"{channelEngineApiBaseUrl}"));
        
        var rateLimitPolicy = Policy
            .RateLimitAsync<RestResponse>(100, TimeSpan.FromSeconds(60), maxBurst: 50);

        var rateLimitRetryPolicy = Policy
            .Handle<RateLimitRejectedException>()
            .OrResult<RestResponse>(r => !r.IsSuccessful)
            .WaitAndRetryForeverAsync(
                sleepDurationProvider: (index, result, context) =>
                {
                    if (result.Exception is RateLimitRejectedException rateLimitException)
                    {
                        _logger.LogWarning("Rate limit exceeded. Retrying after {RetryAfter}", rateLimitException.RetryAfter);
                        return rateLimitException.RetryAfter;
                    } 
                    _logger.LogError("Request failed. Retrying after 60 seconds.");
                    return TimeSpan.FromSeconds(60);
                },
                onRetryAsync: (result, span, context) =>
                {
                    _logger.LogWarning($"Retrying after {span}");
                    return Task.Delay(span);
                } 
            );
        
        var generalRetryPolicy = Policy<RestResponse>.Handle<WebException>()
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt =>
            {
                _logger.LogWarning($"Something went wrong with ChannelEngine API. Retrying attempt {retryAttempt}");
                return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
            });
        
        _policy = Policy.WrapAsync(rateLimitRetryPolicy, rateLimitPolicy, generalRetryPolicy);

        _logger.LogInformation("ChannelEngineClient initialized.");
    }

    public async Task<Result<T>> ExecuteAsync<T>(RestRequest request, CancellationToken cancellationToken) where T : new()
    {
        var restResponseContent = string.Empty;
        try
        {
            request.AddQueryParameter("apikey", _apiKey);
            var restResponse = await _policy
                .ExecuteAsync(() => _restClient.ExecuteAsync(request, cancellationToken));
            
            restResponseContent = restResponse.Content ?? string.Empty;
            if (restResponse.IsSuccessful)
            {
                var response = JsonConvert.DeserializeObject<T>(restResponseContent);
                return Result<T>.CreateSuccess(response);
            }

            var errorObject = JsonConvert.DeserializeObject<Result>(restResponseContent);
            return Result<T>.CreateFailure(restResponse.StatusCode, errorObject?.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to send request. Exception: {exception}", ex.Message);

            return ex switch
            {
                JsonSerializationException => Result<T>.CreateFailure(HttpStatusCode.InternalServerError,
                    new ErrorObject { Text = $"Failed to deserialize response => {restResponseContent}" }),
                
                RateLimitRejectedException => Result<T>.CreateFailure(HttpStatusCode.TooManyRequests,
                    new ErrorObject { Text = "Rate limit exceeded. Please try again later." }),

                _ => Result<T>.CreateFailure(HttpStatusCode.InternalServerError,
                    new ErrorObject { Text = "An error occurred while sending the request." })
            };
        }
    }
    
    public async Task<Result> ExecuteAsync(RestRequest request, CancellationToken cancellationToken)
    {
        var restResponseContent = string.Empty;
        try
        {
            request.AddQueryParameter("apikey", _apiKey);
            var restResponse = await _policy
                .ExecuteAsync(() => _restClient.ExecuteAsync(request, cancellationToken));
            
            restResponseContent = restResponse.Content ?? string.Empty;
            if (restResponse.IsSuccessful)
            {
                return Result.CreateSuccess();
            }

            var errorObject = JsonConvert.DeserializeObject<Result>(restResponseContent);
            return Result.CreateFailure(restResponse.StatusCode, errorObject?.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to send request. Exception: {exception}", ex.Message);

            return ex switch
            {
                JsonSerializationException => Result.CreateFailure(HttpStatusCode.InternalServerError, 
                    new List<ErrorObject> { new() { Text = "Failed to deserialize response." } }),
                
                RateLimitRejectedException => Result.CreateFailure(HttpStatusCode.TooManyRequests,
                    new List<ErrorObject> { new() { Text = "Rate limit exceeded. Please try again later."} }),

                _ => Result.CreateFailure(HttpStatusCode.InternalServerError,
                    new List<ErrorObject> { new() { Text = "An error occurred while sending the request."} })
            };
        }
    }
}