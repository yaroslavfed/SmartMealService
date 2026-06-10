using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableLogging;
using SmartMealService.Wpf.Services.EnvironmentVariables.EnvironmentVariableStore;
using SmartMealService.Wpf.Startup;

namespace SmartMealService.Wpf.Controls.EnvironmentVariablesPanel;

public sealed class EnvironmentVariablesPanelViewModel : ReactiveObject, IDisposable
{
    private readonly IEnvironmentVariableStore        _store;
    private readonly IEnvironmentVariableChangeLogger _logger;
    private readonly CompositeDisposable              _subscriptions = [];
    private readonly IReadOnlyDictionary<string, string> _defaultValues;

    public EnvironmentVariablesPanelViewModel(
        IReadOnlyCollection<string> environmentVariableNames,
        IEnvironmentVariableStore store,
        IEnvironmentVariableChangeLogger logger,
        IReadOnlyDictionary<string, string>? defaultValues = null,
        IReadOnlyDictionary<string, string>? comments = null
    )
    {
        ReactiveUiBootstrapper.EnsureInitialized();
        _store = store;
        _logger = logger;
        _defaultValues = defaultValues ?? new Dictionary<string, string>();
        Comments = comments ?? new Dictionary<string, string>();

        EnvironmentVariables = new(environmentVariableNames.Select(CreateRow));
    }

    public ObservableCollection<EnvironmentVariableRow> EnvironmentVariables { get; }

    private IReadOnlyDictionary<string, string> Comments { get; }

    private EnvironmentVariableRow CreateRow(string name)
    {
        var storedValue = _store.GetValue(name);
        if (storedValue is null)
        {
            storedValue = _defaultValues.GetValueOrDefault(name, string.Empty);
            PersistValue(name, storedValue);
        }

        var row = new EnvironmentVariableRow(name, storedValue, Comments.GetValueOrDefault(name, string.Empty));
        var subscription = row
                           .WhenAnyValue(x => x.Value)
                           .Skip(1)
                           .DistinctUntilChanged()
                           .Subscribe(value =>
                               {
                                   PersistValue(row.Name, value);
                               }
                           );

        _subscriptions.Add(subscription);

        return row;
    }

    private void PersistValue(string name, string value)
    {
        _ = Task.Run(() =>
        {
            _store.SetValue(name, value);
            _logger.LogChanged(name, value);
        });
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }
}
