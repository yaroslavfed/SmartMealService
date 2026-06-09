using FluentAssertions;
using SmartMealService.Wpf.Services;

namespace SmartMealService.Wpf.Tests.Services;

public class UserEnvironmentVariableStoreTests
{
    [Fact]
    public void SetValue_ShouldPersistValueToUserEnvironment()
    {
        var name = $"SMART_MEAL_SERVICE_WPF_STORE_TEST_{Guid.NewGuid():N}";
        var store = new UserEnvironmentVariableStore();

        try
        {
            store.SetValue(name, "test-value");

            Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User)
                .Should().Be("test-value");
            store.GetValue(name).Should().Be("test-value");
        }
        finally
        {
            Environment.SetEnvironmentVariable(name, null, EnvironmentVariableTarget.User);
        }
    }
}
