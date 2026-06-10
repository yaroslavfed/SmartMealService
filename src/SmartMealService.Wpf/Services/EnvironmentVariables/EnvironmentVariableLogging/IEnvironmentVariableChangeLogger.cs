namespace SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableLogging;

public interface IEnvironmentVariableChangeLogger
{
    void LogChanged(string name, string value);
}
