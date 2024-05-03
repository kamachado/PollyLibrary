using System.Net;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using Polly.Timeout;


var fallbackStrategyOptions = new FallbackStrategyOptions<HttpResponseMessage>
{
    FallbackAction = _ =>
    {
        return Outcome.FromResultAsValueTask(new HttpResponseMessage(HttpStatusCode.OK));
    },

    ShouldHandle = arguments => arguments.Outcome switch
    {
        { Exception: HttpRequestException } => PredicateResult.True(),
        { Exception: TimeoutRejectedException } => PredicateResult.True(),
        { Result: HttpResponseMessage response } when response.StatusCode == HttpStatusCode.InternalServerError =>
            PredicateResult.True(),
        _ => PredicateResult.False(),
    },
    OnFallback = _ =>
    {
        Console.WriteLine("Fallback!");
        return default;
    }
};

var retryStrategyOptions = new RetryStrategyOptions<HttpResponseMessage>
{
    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .HandleResult(r => r.StatusCode == HttpStatusCode.InternalServerError)
        .Handle<HttpRequestException>(),

    OnRetry = arguments =>
    {
        Console.WriteLine($"Retrying '{arguments.Outcome.Result?.StatusCode}'...");
        return default;
    },
    Delay = TimeSpan.FromMilliseconds(400),
    BackoffType = DelayBackoffType.Constant,
    MaxRetryAttempts = 3,
};

ResiliencePipeline<HttpResponseMessage> pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddFallback(fallbackStrategyOptions)
    .AddRetry(retryStrategyOptions)
    .AddTimeout(TimeSpan.FromSeconds(1)).Build();

var response = await pipeline.ExecuteAsync(
    async token =>
    {
        await Task.Delay(10000, token);

        // This causes the action fail, thus using the fallback strategy above
        return new HttpResponseMessage(HttpStatusCode.OK);
    },
    CancellationToken.None);

Console.WriteLine($"Response: {response.StatusCode}");