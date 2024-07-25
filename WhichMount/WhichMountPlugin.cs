using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace WhichMount;

#pragma warning disable CA1416

public class WhichMountPlugin : IDalamudPlugin
{
    public string Name => "Which Mount";

    private Configuration _configuration;
    private ContextMenuHandler _contextMenuHandler;

    private readonly ConfigWindow _configWindow;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly IChatGui _chatGui;
    private readonly IDataManager _dataManager;
    private readonly IObjectTable _objectTable;
    private readonly IContextMenu _contextMenu;
    
    public WhichMountPlugin(IDalamudPluginInterface pluginInterface) 
    {
        _configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration(pluginInterface);

        pluginInterface.Create<Service>();
        _pluginInterface = Service.Interface;
        _chatGui = Service.ChatGui;
        _dataManager = Service.DataManager;
        _objectTable = Service.ObjectTable;
        _contextMenu = Service.ContextMenu;

        _contextMenuHandler = new ContextMenuHandler(_pluginInterface, _chatGui, _dataManager, _objectTable, _contextMenu, _configuration);
        _configWindow = new ConfigWindow(_pluginInterface, this, _configuration);
    }
    
    public void Dispose()
    {
        _contextMenuHandler.Dispose();
        _configWindow.Dispose();
    }
}

#pragma warning restore CA1416
