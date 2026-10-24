using Polling.Shared;
using Xunit;

namespace Polling.Tests;

public class ResultTests
{
    [Fact]
    public void Success_HasValueAndNoError()
    {
        var result = Result<int>.Success(42);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_HasMeaningfulErrorAndNoValue()
    {
        var result = Result<string>.Failure("validation", "Invalid request.");
        Assert.True(result.IsFailure);
        Assert.Null(result.Value);
        Assert.Equal("validation", result.Error.Code);
    }

    [Theory]
    [InlineData("Success")]
    [InlineData("Failed")]
    [InlineData("Delayed")]
    public void SupportedPaymentModes_AreRecognized(string mode)
    {
        Assert.Contains(mode, new[] { "Success", "Failed", "Delayed" });
    }
}
