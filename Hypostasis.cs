global using Dalamud;
global using Hypostasis;
using Dalamud.Plugin;

namespace Hypostasis;

public static class Hypostasis
{
    public static void Initialize(DalamudPluginInterface pluginInterface)
    {
        DalamudApi.Initialize(pluginInterface);
    }

    public static void Dispose()
    {
        Memory.Dispose();
    }
}