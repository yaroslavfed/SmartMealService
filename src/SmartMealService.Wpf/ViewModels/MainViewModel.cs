using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using SmartMealService.Wpf.Services;
using SmartMealService.Wpf.Startup;

namespace SmartMealService.Wpf.ViewModels;

public sealed class MainViewModel : ReactiveObject, IDisposable
{
    private readonly IEnvironmentVariableStore _store;
    private readonly IEnvironmentVariableChangeLogger _logger;
    private readonly CompositeDisposable _subscriptions = [];

    public MainViewModel(
        IReadOnlyCollection<string> environmentVariableNames,
        IEnvironmentVariableStore store,
        IEnvironmentVariableChangeLogger logger,
        IReadOnlyDictionary<string, string>? comments = null)
    {
        ReactiveUiBootstrapper.EnsureInitialized();
        _store = store;
        _logger = logger;
        Comments = comments ?? new Dictionary<string, string>();

        EnvironmentVariables = new ObservableCollection<EnvironmentVariableRow>(
            environmentVariableNames.Select(CreateRow));
    }

    public ObservableCollection<EnvironmentVariableRow> EnvironmentVariables { get; }

    private IReadOnlyDictionary<string, string> Comments { get; }

    private EnvironmentVariableRow CreateRow(string name)
    {
        var storedValue = _store.GetValue(name);
        if (storedValue is null)
        {
            storedValue = string.Empty;
            _store.SetValue(name, storedValue);
            _logger.LogChanged(name, storedValue);
        }

        var row = new EnvironmentVariableRow(name, storedValue, Comments.GetValueOrDefault(name, string.Empty));
        var subscription = row.WhenAnyValue(x => x.Value)
            .Skip(1)
            .Subscribe(value =>
            {
                _store.SetValue(row.Name, value);
                _logger.LogChanged(row.Name, value);
            });

        _subscriptions.Add(subscription);

        return row;
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }
}
