using ReactiveUI.Builder;

namespace SmartMealService.Wpf.Startup;

static internal class ReactiveUiBootstrapper
{
    private static readonly object SyncRoot = new();
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized)
            return;

        lock (SyncRoot)
        {
            if (_initialized)
                return;

            RxAppBuilder.CreateReactiveUIBuilder()
                .WithCoreServices()
                .WithWpf()
                .BuildApp();

            _initialized = true;
        }
    }
}
