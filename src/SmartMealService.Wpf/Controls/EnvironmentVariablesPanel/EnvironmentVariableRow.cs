using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace SmartMealService.Wpf.Controls.EnvironmentVariablesPanel;

public sealed class EnvironmentVariableRow : ReactiveObject
{
    public EnvironmentVariableRow(string name, string value, string comment)
    {
        Name = name;
        Value = value;
        Comment = comment;
    }

    public string Name { get; }

    [Reactive]
    public string Value { get; set; }

    public string Comment { get; }
}
