using IntegrationBus.Core.Models;
using IntegrationBus.Services;

namespace IntegrationBus.Tests;

[TestClass]
public class ExternalDispatcherTests
{
    private static Order SampleOrder() => new()
    {
        Id = "001",
        Status = "Active",
        Amount = 1500,
        Product = "Widget"
    };

    [TestMethod]
    public async Task DispatchAsync_SuccessfulCall_DoesNotThrow()
    {
        var handler = new FakeHttpHandler(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var dispatcher = new ExternalDispatcher(client);

        await dispatcher.DispatchAsync(SampleOrder());
    }

    [TestMethod]
    public async Task DispatchAsync_ServerError_RetriesBeforeTimeout()
    {
        var handler = new FakeHttpHandler(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var dispatcher = new ExternalDispatcher(client);

        // The 5s timeout fires before all retries complete (backoff: 1s+2s+4s = 7s)
        // so the dispatcher throws a cancellation-derived exception
        var ex = await Assert.ThrowsExceptionAsync<TaskCanceledException>(
            () => dispatcher.DispatchAsync(SampleOrder()));

        // At least 1 retry should have been attempted before timeout
        Assert.IsTrue(handler.CallCount > 1, $"Expected retries, got {handler.CallCount} calls");
    }

    [TestMethod]
    public async Task DispatchAsync_CancellationRequested_Throws()
    {
        var handler = new FakeHttpHandler(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var dispatcher = new ExternalDispatcher(client);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsExceptionAsync<TaskCanceledException>(
            () => dispatcher.DispatchAsync(SampleOrder(), cts.Token));
    }

    [TestMethod]
    public async Task DispatchAsync_RespectsMaxConcurrency()
    {
        var delay = TimeSpan.FromMilliseconds(200);
        var handler = new FakeHttpHandler(new HttpResponseMessage(System.Net.HttpStatusCode.OK), delay);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var dispatcher = new ExternalDispatcher(client);

        // Launch 10 requests — only 5 should run concurrently
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => dispatcher.DispatchAsync(SampleOrder()))
            .ToList();

        await Task.WhenAll(tasks);

        Assert.AreEqual(10, handler.CallCount);
        Assert.IsTrue(handler.MaxConcurrent <= 5, $"Max concurrent was {handler.MaxConcurrent}, expected <= 5");
    }

    /// <summary>
    /// Fake HTTP handler that returns a fixed response, optionally with a delay.
    /// Tracks call count and max concurrent requests.
    /// </summary>
    private class FakeHttpHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        private readonly TimeSpan _delay;
        private int _callCount;
        private int _concurrent;
        private int _maxConcurrent;

        public int CallCount => _callCount;
        public int MaxConcurrent => _maxConcurrent;

        public FakeHttpHandler(HttpResponseMessage response, TimeSpan delay = default)
        {
            _response = response;
            _delay = delay;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var current = Interlocked.Increment(ref _concurrent);
            InterlockedMax(ref _maxConcurrent, current);
            Interlocked.Increment(ref _callCount);

            try
            {
                if (_delay > TimeSpan.Zero)
                    await Task.Delay(_delay, cancellationToken);

                return new HttpResponseMessage(_response.StatusCode);
            }
            finally
            {
                Interlocked.Decrement(ref _concurrent);
            }
        }

        private static void InterlockedMax(ref int target, int value)
        {
            int current;
            do { current = target; }
            while (value > current && Interlocked.CompareExchange(ref target, value, current) != current);
        }
    }
}
