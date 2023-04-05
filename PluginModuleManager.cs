using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;

namespace Hypostasis;

public static class PluginModuleManager
{
    private static readonly Dictionary<Type, PluginModule> pluginModules = new();

    public static bool Initialize()
    {
        var succeeded = true;

        foreach (var t in Util.Assembly.GetTypes<PluginModule>())
        {
            var pluginModule = (PluginModule)Activator.CreateInstance(t);
            if (pluginModule == null) continue;

            if (pluginModule.IsValid)
            {
                if (pluginModule.ShouldEnable)
                    ToggleOrInvalidateModule(pluginModule, Hypostasis.IsDebug);
            }
            else
            {
                PluginLog.Warning($"{t} failed to load!");
                succeeded = false;
            }

            pluginModules.Add(t, pluginModule);
        }

        return succeeded;
    }

    public static T GetModule<T>() where T : PluginModule => (T)pluginModules[typeof(T)];

    public static void CheckModules()
    {
        foreach (var (_, pluginModule) in pluginModules.Where(kv => kv.Value.IsValid && kv.Value.ShouldEnable != kv.Value.IsEnabled))
            ToggleOrInvalidateModule(pluginModule, true);
    }

    public static void ToggleOrInvalidateModule(PluginModule pluginModule, bool logInfo)
    {
        try
        {
            pluginModule.Toggle();
            if (logInfo)
                PluginLog.Information(pluginModule.IsEnabled ? $"Enabled plugin module: {pluginModule}" : $"Disabled plugin module: {pluginModule}");
        }
        catch (Exception e)
        {
            PluginLog.Error(e, $"Error in plugin module: {pluginModule}");
            pluginModule.IsValid = false;
        }
    }

    public static void Dispose()
    {
        foreach (var (_, pluginModule) in pluginModules.Where(kv => kv.Value.IsValid && kv.Value.IsEnabled))
            pluginModule.Toggle();
    }
}