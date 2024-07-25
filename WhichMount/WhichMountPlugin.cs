using Dalamud.Plugin;

namespace WhichMount;

#pragma warning disable CA1416

public class WhichMountMain : IDalamudPlugin
{
    public string Name => "WM";
    
    private ContextMenuHandler contextMenuHandler;
    
    private bool drawConfigWindow;
    
    public WhichMountMain(IDalamudPluginInterface pluginInterface) 
    {
        pluginInterface.Create<Service>();

        contextMenuHandler = new ContextMenuHandler();
        contextMenuHandler.Enable();

        pluginInterface.UiBuilder.OpenConfigUi += () => {
            drawConfigWindow = true;
        };

        pluginInterface.UiBuilder.Draw += this.BuildUI;
    }
    
    private void BuildUI() 
    {
        
    }
    
    public void Dispose()
    {
        contextMenuHandler.Dispose();
    }
}

#pragma warning restore CA1416
