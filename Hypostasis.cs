global using Dalamud;
global using Hypostasis;
global using Hypostasis.Dalamud;
global using Hypostasis.Game;
using Dalamud.Plugin;

namespace Hypostasis;

public static class Hypostasis
{
    public static string PluginName { get; private set; }
    public static bool FailState { get; set; }
    public static bool Debug { get; }
#if DEBUG
    = true;
#endif

    public static void Initialize(string pluginName, DalamudPluginInterface pluginInterface)
    {
        PluginName = pluginName;
        DalamudApi.Initialize(pluginInterface);
        Common.Initialize();
        if (!Debug) return;
        IPC.Initialize(pluginName);
    }

    public static void Dispose()
    {
        if (Debug)
            IPC.Dispose();
        DalamudApi.Dispose();
        Common.Dispose();
        ASMReplacer.DisposeAll();
        FailState = false;
    }
}