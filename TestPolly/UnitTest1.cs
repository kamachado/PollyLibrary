using System;
using Xunit;

public class ResiliencePipelineBuilderTests
{
    [Fact]
    public void ResiliencePipelineBuilder_Build_ReturnsCorrectPipelineDescriptor()
    {
        // Arrange
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 4
            })
            .AddTimeout(TimeSpan.FromSeconds(1))
            .Build();

        // Act
        var descriptor = pipeline.GetPipelineDescriptor();

        // Assert
        Assert.Equal(2, descriptor.Strategies.Count);

        // Verify the retry settings.
        var retryOptions = Assert.IsType<RetryStrategyOptions>(descriptor.Strategies[0].Options);
        Assert.Equal(4, retryOptions.MaxRetryAttempts);

        // Confirm the timeout settings.
        var timeoutOptions = Assert.IsType<TimeoutStrategyOptions>(descriptor.Strategies[1].Options);
        Assert.Equal(TimeSpan.FromSeconds(1), timeoutOptions.Timeout);
    }
}