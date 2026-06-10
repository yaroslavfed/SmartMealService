using FluentAssertions;
using Moq;
using SmartMealService.Wpf.Services.EnvironmentVariables;
using SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableChangeNotifier;
using SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableStore;

namespace SmartMealService.Wpf.Tests.Services.EnvironmentVariables;

public class UserEnvironmentVariableStoreTests
{
    [Fact]
    public void SetValue_ShouldPersistValueToUserEnvironment()
    {
        var name = $"SMART_MEAL_SERVICE_WPF_STORE_TEST_{Guid.NewGuid():N}";
        var changeNotifier = new Mock<IEnvironmentVariableChangeNotifier>();
        var store = new UserEnvironmentVariableStore(changeNotifier.Object);

        try
        {
            store.SetValue(name, "test-value");

            Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User)
                .Should().Be("test-value");
            store.GetValue(name).Should().Be("test-value");
            changeNotifier.Verify(n => n.NotifyChanged(), Times.Once);
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null, EnvironmentVariableTarget.User);
        }
    }
}
