using NLog;

namespace SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableLogging;

public sealed class NLogEnvironmentVariableChangeLogger : IEnvironmentVariableChangeLogger
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public void LogChanged(string name, string value)
    {
        Logger.Info("Переменная среды {name} изменена. Новое значение: {value}", name, value);
    }
}
