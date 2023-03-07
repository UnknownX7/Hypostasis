using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Logging;

namespace Hypostasis;

public static class PluginModuleManager
{
    private static readonly List<PluginModule> pluginModules = new();

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

            t.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, pluginModule);
            pluginModules.Add(pluginModule);
        }

        return succeeded;
    }

    public static void CheckModules()
    {
        foreach (var pluginModule in pluginModules.Where(pluginModule => pluginModule.IsValid && pluginModule.ShouldEnable != pluginModule.IsEnabled))
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
        foreach (var pluginModule in pluginModules.Where(pluginModule => pluginModule.IsValid && pluginModule.IsEnabled))
            pluginModule.Toggle();
    }
}