global using Dalamud;
global using Hypostasis;
global using Hypostasis.Dalamud;
global using Hypostasis.Game;
using Dalamud.Plugin;

namespace Hypostasis;

public static class Hypostasis
{
    public static string PluginName { get; private set; }

    public static void Initialize(string pluginName, DalamudPluginInterface pluginInterface)
    {
        PluginName = pluginName;
        DalamudApi.Initialize(pluginInterface);
        Common.Initialize();
    }

    public static void Dispose()
    {
        DalamudApi.Dispose();
        Common.Dispose();
        ASMReplacer.DisposeAll();
    }
}