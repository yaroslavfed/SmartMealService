using SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableChangeNotifier;

namespace SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableStore;

public sealed class UserEnvironmentVariableStore : IEnvironmentVariableStore
{
    private readonly IEnvironmentVariableChangeNotifier _changeNotifier;

    public UserEnvironmentVariableStore(IEnvironmentVariableChangeNotifier changeNotifier)
    {
        _changeNotifier = changeNotifier;
    }

    public string? GetValue(string name)
    {
        return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
    }

    public void SetValue(string name, string value)
    {
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
        _changeNotifier.NotifyChanged();
    }
}
