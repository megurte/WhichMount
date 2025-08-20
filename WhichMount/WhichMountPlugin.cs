using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DalamudInjector;
using WhichMount.ComponentInjector;
using WhichMount.Models;
using WhichMount.UI;

namespace WhichMount;

#pragma warning disable CA1416

public class WhichMountPlugin : IDalamudPlugin
{
    public string Name => "Which Mount";

    private readonly Configuration _configuration;
    private readonly ServiceInstaller _serviceInstaller;
    private readonly ServiceManager _service;
    private readonly ComponentContainer _container;

    public WhichMountPlugin(IDalamudPluginInterface pluginInterface)
    {
        _configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration(pluginInterface);

        _serviceInstaller = new ServiceInstaller(pluginInterface);
        _service = _serviceInstaller.Service;
        _container = _service.Container;
        _container.BindInstance(this);
        _container.BindInstance(pluginInterface);
        _container.BindInstance(_configuration);

        _container.Bind<CashContainer>();
        _container.Bind<ContextMenuHandler>();
        _container.Bind<MountListWindow>();
        _container.Bind<MountInfoTooltip>();
        _container.Bind<ConfigWindow>();
        _container.Bind<CommandHandler>();
    }
    
    public void Dispose()
    {
        _container.Dispose();
    }
}

#pragma warning restore CA1416
