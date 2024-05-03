using Polly;
using Polly.Retry;
using Polly.Testing;
using Polly.Timeout;
using Xunit;

namespace TestProject1;

public class ResiliencePipelineBuilderTests
{
    [Fact]
    public void ResiliencePipelineBuilder_Build_ReturnsCorrectPipelineDescriptor()
    {
    
        ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 4
            })
            .AddTimeout(TimeSpan.FromSeconds(1))
            .Build();
        
        ResiliencePipelineDescriptor descriptor = pipeline.GetPipelineDescriptor();
        
        Assert.Equal(2, descriptor.Strategies.Count);

        var retryOptions = Assert.IsType<RetryStrategyOptions>(descriptor.Strategies[0].Options);
        Assert.Equal(4, retryOptions.MaxRetryAttempts);
        
        var timeoutOptions = Assert.IsType<TimeoutStrategyOptions>(descriptor.Strategies[1].Options);
        Assert.Equal(TimeSpan.FromSeconds(1), timeoutOptions.Timeout);
    }
}