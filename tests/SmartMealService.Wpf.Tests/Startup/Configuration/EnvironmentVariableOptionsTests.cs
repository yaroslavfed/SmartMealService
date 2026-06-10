using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SmartMealService.Wpf.Startup.Configuration;

namespace SmartMealService.Wpf.Tests.Startup.Configuration;

public class EnvironmentVariableOptionsTests
{
    [Fact]
    public void FromConfiguration_ShouldReadEnvironmentVariableNames()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EnvironmentVariables:Names:0"] = "SMS_HTTP_BASE_URL",
                ["EnvironmentVariables:Names:1"] = "SMS_HTTP_USERNAME",
                ["EnvironmentVariables:Names:2"] = "SMS_HTTP_PASSWORD",
                ["EnvironmentVariables:Defaults:SMS_HTTP_BASE_URL"] = "http://localhost:5000/",
                ["EnvironmentVariables:Comments:SMS_HTTP_BASE_URL"] = "Адрес SMS HTTP-сервера"
            })
            .Build();

        var options = EnvironmentVariableOptions.FromConfiguration(configuration);

        options.Names.Should().Equal("SMS_HTTP_BASE_URL", "SMS_HTTP_USERNAME", "SMS_HTTP_PASSWORD");
        options.Defaults.Should().ContainKey("SMS_HTTP_BASE_URL")
            .WhoseValue.Should().Be("http://localhost:5000/");
        options.Comments.Should().ContainKey("SMS_HTTP_BASE_URL")
            .WhoseValue.Should().Be("Адрес SMS HTTP-сервера");
    }

    [Fact]
    public void FromConfiguration_ShouldThrow_WhenNoNamesConfigured()
    {
        var configuration = new ConfigurationBuilder().Build();

        var act = () => EnvironmentVariableOptions.FromConfiguration(configuration);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*EnvironmentVariables:Names*");
    }
}
