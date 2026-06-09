namespace SmartMealService.Wpf.Services;

public sealed class UserEnvironmentVariableStore : IEnvironmentVariableStore
{
    public string? GetValue(string name)
    {
        return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);
    }

    public void SetValue(string name, string value)
    {
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
    }
}
