namespace SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableStore;

public interface IEnvironmentVariableStore
{
    string? GetValue(string name);

    void SetValue(string name, string value);
}
