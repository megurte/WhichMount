using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

#pragma warning disable CA1416

namespace WhichMount;

internal class Service
{
    // Inject services
    [PluginService] internal static IDalamudPluginInterface Interface { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IContextMenu ContextMenu { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
}

#pragma warning restore CA1416
