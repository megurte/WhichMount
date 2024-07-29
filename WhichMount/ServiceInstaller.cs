using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DalamudInjector;

#pragma warning disable CA1416

namespace WhichMount;

public class ServiceInstaller
{
    public readonly ServiceManager Service;

    public ServiceInstaller(IDalamudPluginInterface pluginInterface)
    {
        Service = new ServiceManager();
        Service.AddExistingService(pluginInterface);
        Service.AddExistingService(pluginInterface.UiBuilder);
        Service.AddDalamudService<IChatGui>(pluginInterface);
        Service.AddDalamudService<IDataManager>(pluginInterface);
        Service.AddDalamudService<IObjectTable>(pluginInterface);
        Service.AddDalamudService<IContextMenu>(pluginInterface);
        Service.AddDalamudService<ICommandManager>(pluginInterface);
        Service.CreateProvider();
    }
}

#pragma warning restore CA1416
