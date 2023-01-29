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

    public static void Initialize(string pluginName, DalamudPluginInterface pluginInterface)
    {
        PluginName = pluginName;
        DalamudApi.Initialize(pluginInterface);
        Common.Initialize();
#if DEBUG
        Debug.Initialize(pluginName);
#endif
    }

    public static void Dispose(bool failed)
    {
#if DEBUG
        if (!failed)
            Debug.Dispose();
#endif
        DalamudApi.Dispose();
        Common.Dispose();
        AsmEdit.DisposeAll();
    }
}