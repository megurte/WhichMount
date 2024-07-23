using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace WhichMount;

public class WhichMountMain : IDalamudPlugin
{
    public string Name => "WhichMount";

    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] public static IGameInteropProvider HookProvider { get; private set; } = null!;
    [PluginService] public static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService] internal static IContextMenu ContextMenu { get; private set; } = null!;
    
    private ContextMenuHandler contextMenuHandler;

    public void Initialize(IDalamudPluginInterface pluginInterface, IContextMenu contextMenu, IChatGui chatGui)
    {
        PluginInterface = pluginInterface;
        ContextMenu = contextMenu;
        ChatGui = chatGui;

        contextMenuHandler = new ContextMenuHandler(PluginInterface, ContextMenu, ChatGui);
    }

    public void Dispose()
    {
        contextMenuHandler.Dispose();
    }

    private void GetMountData()
    {
        
    }
}
