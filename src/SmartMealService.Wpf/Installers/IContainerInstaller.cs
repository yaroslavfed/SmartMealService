using Autofac;

namespace SmartMealService.Wpf.Installers;

public interface IContainerInstaller<TInstaller>
    where TInstaller : IContainerInstaller<TInstaller>
{
    static abstract void Install(ContainerBuilder builder);
}
