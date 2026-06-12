using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using WhichMount.ComponentInjector;
using WhichMount.UI;

namespace WhichMount;

[InjectFields]
public class CommandHandler : IPluginComponent, IInitializable
{
    private const string ConfigCommand = "/mountsconfig";
    private const string WhichMountListWindowCommand = "/mountlist";
     
    [Inject] private IDalamudPluginInterface _pluginInterface;
    [Inject] private ICommandManager _commandManager;
    [Inject] private ConfigWindow _configWindow;
    [Inject] private MountListWindow _mountListWindow;

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
        _commandManager.AddHandler(WhichMountListWindowCommand, new CommandInfo((_, _) => _mountListWindow.Show())
        {
            HelpMessage = "Show mount database."
        });
    }
    
    public void Release()
    {
        _pluginInterface.UiBuilder.OpenConfigUi -= _configWindow.Show;
        _commandManager.RemoveHandler(ConfigCommand);
        _commandManager.RemoveHandler(WhichMountListWindowCommand);
    }
}
