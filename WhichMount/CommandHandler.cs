using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using WhichMount.ComponentInjector;
using WhichMount.UI;

namespace WhichMount;

public class CommandHandler : IPluginComponent, IInitializable
{
    private const string ConfigCommand = "/mountsconfig";
    private const string MountDataBaseCommand = "/mountlist";
    
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly ICommandManager _commandManager;
    private readonly ConfigWindow _configWindow;
    private readonly MountListWindow _mountListWindow;

    public CommandHandler(IDalamudPluginInterface pluginInterface, ICommandManager commandManager, ConfigWindow configWindow, MountListWindow mountListWindow)
    {
        _pluginInterface = pluginInterface;
        _commandManager = commandManager;
        _configWindow = configWindow;
        _mountListWindow = mountListWindow;
    }
    
    public void Initialize()
    {
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
    
    public void Release()
    {
        _pluginInterface.UiBuilder.OpenConfigUi -= _configWindow.Show;
        _commandManager.RemoveHandler("/mountsconfig");
        _commandManager.RemoveHandler("/mountlist");
    }
}
