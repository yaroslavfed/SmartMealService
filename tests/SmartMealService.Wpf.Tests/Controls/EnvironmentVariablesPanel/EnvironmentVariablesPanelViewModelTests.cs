using FluentAssertions;
using Moq;
using SmartMealService.Wpf.Controls.EnvironmentVariablesPanel;
using SmartMealService.Wpf.Services.EnvironmentVariables;
using SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableLogging;
using SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableStore;

namespace SmartMealService.Wpf.Tests.Controls.EnvironmentVariablesPanel;

public class EnvironmentVariablesPanelViewModelTests
{
    [Fact]
    public void Constructor_ShouldLoadConfiguredEnvironmentVariables()
    {
        var store = new Mock<IEnvironmentVariableStore>();
        var logger = new Mock<IEnvironmentVariableChangeLogger>();
        store.Setup(s => s.GetValue("SMS_HTTP_BASE_URL")).Returns("http://localhost:5000");
        store.Setup(s => s.GetValue("SMS_HTTP_USERNAME")).Returns((string?)null);

        var viewModel = new EnvironmentVariablesPanelViewModel(
            ["SMS_HTTP_BASE_URL", "SMS_HTTP_USERNAME"],
            store.Object,
            logger.Object);

        viewModel.EnvironmentVariables.Should().HaveCount(2);
        viewModel.EnvironmentVariables[0].Name.Should().Be("SMS_HTTP_BASE_URL");
        viewModel.EnvironmentVariables[0].Value.Should().Be("http://localhost:5000");
        viewModel.EnvironmentVariables[1].Name.Should().Be("SMS_HTTP_USERNAME");
        viewModel.EnvironmentVariables[1].Value.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ShouldUseConfiguredComments()
    {
        var store = new Mock<IEnvironmentVariableStore>();
        var logger = new Mock<IEnvironmentVariableChangeLogger>();
        store.Setup(s => s.GetValue("SMS_HTTP_BASE_URL")).Returns("http://localhost:5000");
        var comments = new Dictionary<string, string>
        {
            ["SMS_HTTP_BASE_URL"] = "Адрес SMS HTTP-сервера"
        };

        var viewModel = new EnvironmentVariablesPanelViewModel(
            ["SMS_HTTP_BASE_URL"],
            store.Object,
            logger.Object,
            comments: comments);

        viewModel.EnvironmentVariables[0].Comment.Should().Be("Адрес SMS HTTP-сервера");
    }

    [Fact]
    public void Constructor_ShouldInitializeMissingVariablesWithDefaultValue()
    {
        var store = new Mock<IEnvironmentVariableStore>();
        var logger = new Mock<IEnvironmentVariableChangeLogger>();
        store.Setup(s => s.GetValue("SMS_HTTP_PASSWORD")).Returns((string?)null);

        var defaultValues = new Dictionary<string, string>
        {
            ["SMS_HTTP_PASSWORD"] = "testpass"
        };

        _ = new EnvironmentVariablesPanelViewModel(
            ["SMS_HTTP_PASSWORD"],
            store.Object,
            logger.Object,
            defaultValues);

        SpinWait.SpinUntil(() =>
        {
            try
            {
                store.Verify(s => s.SetValue("SMS_HTTP_PASSWORD", "testpass"), Times.Once);
                logger.Verify(l => l.LogChanged("SMS_HTTP_PASSWORD", "testpass"), Times.Once);
                return true;
            }
            catch (MockException)
            {
                return false;
            }
        }, TimeSpan.FromSeconds(1)).Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldUseEmptyDefaultValue_WhenDefaultIsNotConfigured()
    {
        var store = new Mock<IEnvironmentVariableStore>();
        var logger = new Mock<IEnvironmentVariableChangeLogger>();
        store.Setup(s => s.GetValue("SMS_HTTP_PASSWORD")).Returns((string?)null);

        _ = new EnvironmentVariablesPanelViewModel(["SMS_HTTP_PASSWORD"], store.Object, logger.Object);

        SpinWait.SpinUntil(() =>
        {
            try
            {
                store.Verify(s => s.SetValue("SMS_HTTP_PASSWORD", ""), Times.Once);
                logger.Verify(l => l.LogChanged("SMS_HTTP_PASSWORD", ""), Times.Once);
                return true;
            }
            catch (MockException)
            {
                return false;
            }
        }, TimeSpan.FromSeconds(1)).Should().BeTrue();
    }

    [Fact]
    public void EnvironmentVariableValueChange_ShouldPersistAndLogNewValue()
    {
        var store = new Mock<IEnvironmentVariableStore>();
        var logger = new Mock<IEnvironmentVariableChangeLogger>();
        store.Setup(s => s.GetValue("SMS_HTTP_BASE_URL")).Returns("http://localhost:5000");
        var viewModel = new EnvironmentVariablesPanelViewModel(["SMS_HTTP_BASE_URL"], store.Object, logger.Object);

        viewModel.EnvironmentVariables[0].Value = "http://localhost:5001";

        SpinWait.SpinUntil(() =>
        {
            try
            {
                store.Verify(s => s.SetValue("SMS_HTTP_BASE_URL", "http://localhost:5001"), Times.Once);
                logger.Verify(l => l.LogChanged("SMS_HTTP_BASE_URL", "http://localhost:5001"), Times.Once);
                return true;
            }
            catch (MockException)
            {
                return false;
            }
        }, TimeSpan.FromSeconds(1)).Should().BeTrue();
    }
}
