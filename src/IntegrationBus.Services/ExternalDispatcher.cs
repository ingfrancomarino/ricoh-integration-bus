using System.Net.Http.Json;
using IntegrationBus.Core.Interfaces;
using IntegrationBus.Core.Models;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace IntegrationBus.Services;

public class ExternalDispatcher : IExternalDispatcher
{
    private readonly HttpClient _httpClient;
    private readonly ResiliencePipeline _pipeline;
    private readonly SemaphoreSlim _throttle = new(5);

    public ExternalDispatcher(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder()
                    .Handle<HttpRequestException>()
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(30)
            })
            .Build();
    }

    public async Task DispatchAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _throttle.WaitAsync(cancellationToken);
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            await _pipeline.ExecuteAsync(async token =>
            {
                await _httpClient.PostAsJsonAsync("/api/orders", order, token);
            }, cts.Token);
        }
        finally
        {
            _throttle.Release();
        }
    }
}
