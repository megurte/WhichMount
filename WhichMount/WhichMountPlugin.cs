using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DalamudInjector;
using WhichMount.UI;
using WhichMount.Utils;

namespace WhichMount;

#pragma warning disable CA1416

public class WhichMountPlugin : IDalamudPlugin
{
    public string Name => "Which Mount";

    private readonly Configuration _configuration;
    private readonly ContextMenuHandler _contextMenuHandler;
    private readonly ServiceInstaller _serviceInstaller;
    private readonly ServiceManager _service;
    private readonly ConfigWindow _configWindow;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly IChatGui _chatGui;
    private readonly IDataManager _dataManager;
    private readonly IObjectTable _objectTable;
    private readonly IContextMenu _contextMenu;
    private readonly ICommandManager _commandManager;
    
    public WhichMountPlugin(IDalamudPluginInterface pluginInterface)
    {
        _pluginInterface = pluginInterface;
        _configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration(pluginInterface);

        _serviceInstaller = new ServiceInstaller(pluginInterface);
        _service = _serviceInstaller.Service;
        
        _chatGui = _service.GetService<IChatGui>();
        _dataManager = _service.GetService<IDataManager>();
        _objectTable = _service.GetService<IObjectTable>();
        _contextMenu = _service.GetService<IContextMenu>();
        _commandManager = _service.GetService<ICommandManager>();

        _chatGui.Print("Init");
        ParseMounts();
        _contextMenuHandler = new ContextMenuHandler(_pluginInterface, _chatGui, _dataManager, _objectTable, _contextMenu, _configuration);
        _configWindow = new ConfigWindow(_pluginInterface, this, _configuration, _commandManager);
    }

    public async void ParseMounts()
    {
        var parser = new Parse();
        await parser.Main(_dataManager);
    }
    
    public void Dispose()
    {
        _contextMenuHandler.Dispose();
        _configWindow.Dispose();
    }
}

#pragma warning restore CA1416
