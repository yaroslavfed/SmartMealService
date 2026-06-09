namespace SmartMealService.Wpf.Services;

public interface IEnvironmentVariableStore
{
    string? GetValue(string name);

    void SetValue(string name, string value);
}
