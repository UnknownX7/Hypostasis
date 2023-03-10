global using Dalamud;
global using Hypostasis;
global using Hypostasis.Dalamud;
global using Hypostasis.Game;
using Dalamud.Plugin;

namespace Hypostasis;

public static class Hypostasis
{
    public enum PluginState
    {
        Loading,
        Loaded,
        Unloading,
        Unloaded,
        Failed
    }

    public static string PluginName { get; private set; }
    public static PluginState State { get; set; }
    public static bool IsDebug { get; }
#if DEBUG
        = true;
#endif

    public static void Initialize(IDalamudPlugin plugin, DalamudPluginInterface pluginInterface)
    {
        PluginName = plugin.Name;
        DalamudApi.Initialize(pluginInterface);
        Common.Initialize();
        Debug.Initialize(plugin);
    }

    public static void Dispose(bool failed)
    {
        if (!failed)
            Debug.Dispose();
        PluginModuleManager.Dispose();
        DalamudApi.Dispose();
        Common.Dispose();
        AsmPatch.DisposeAll();
    }
}