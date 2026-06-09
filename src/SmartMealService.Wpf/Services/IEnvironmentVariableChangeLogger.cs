namespace SmartMealService.Wpf.Services;

public interface IEnvironmentVariableChangeLogger
{
    void LogChanged(string name, string value);
}
