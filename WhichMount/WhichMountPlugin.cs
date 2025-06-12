using Dalamud.Game.Command;
using Dalamud.Interface.Textures;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DalamudInjector;
using WhichMount.Models;
using WhichMount.UI;

namespace WhichMount;

#pragma warning disable CA1416

public class WhichMountPlugin : IDalamudPlugin
{
    public string Name => "Which Mount";

    private const string ConfigCommand = "/mountsconfig";
    private const string MountDataBaseCommand = "/mountlist";

    private readonly Configuration _configuration;
    private readonly ContextMenuHandler _contextMenuHandler;
    private readonly ServiceInstaller _serviceInstaller;
    private readonly ServiceManager _service;
    private readonly ConfigWindow _configWindow;
    private readonly MountListWindow _mountListWindow;
    private readonly CashContainer _cashContainer;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly IChatGui _chatGui;
    private readonly IDataManager _dataManager;
    private readonly IObjectTable _objectTable;
    private readonly IContextMenu _contextMenu;
    private readonly ITextureProvider _textureProvider;

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
        _textureProvider = _service.GetService<ITextureProvider>();
        _cashContainer = new CashContainer(_dataManager);
        _contextMenuHandler = new ContextMenuHandler(_pluginInterface, _chatGui, _dataManager, _objectTable, _contextMenu, _configuration, _cashContainer);
        _mountListWindow = new MountListWindow(_pluginInterface, _cashContainer, _textureProvider, _chatGui);
        _configWindow = new ConfigWindow(_pluginInterface, this, _configuration);

        RegisterCommands();
    }

    private void RegisterCommands()
    {
        _pluginInterface.UiBuilder.OpenConfigUi += _configWindow.Show;

        _commandManager.AddHandler(ConfigCommand, new CommandInfo((_, _) => _configWindow.Show())
        {
            HelpMessage = "Open mount search configuration."
        });
        _commandManager.AddHandler(MountDataBaseCommand, new CommandInfo((_, _) => _mountListWindow.Show())
        {
            HelpMessage = "Show mount database."
        });
    }
    
    public void Dispose()
    {
        _contextMenuHandler.Dispose();
        _configWindow.Dispose();
        _mountListWindow.Dispose();
        _cashContainer.Dispose();

        _pluginInterface.UiBuilder.OpenConfigUi -= _configWindow.Show;
        _commandManager.RemoveHandler("/mountsconfig");
        _commandManager.RemoveHandler("/mountlist");
    }
}

#pragma warning restore CA1416
