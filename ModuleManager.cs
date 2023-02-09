using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Logging;

namespace Hypostasis;

public static class ModuleManager
{
    private static readonly List<Module> modules = new();

    public static void Initialize()
    {
        foreach (var t in Util.AssemblyTypes.Where(t => t.IsSubclassOf(typeof(Module)) && !t.IsAbstract))
        {
            var module = (Module)Activator.CreateInstance(t);
            if (module == null) continue;

            if (module.IsValid)
            {
                if (module.ShouldEnable)
                    ToggleOrInvalidateModule(module, Hypostasis.IsDebug);
            }
            else
            {
                PluginLog.Warning($"{t}.Initialize() failed!");
            }

            t.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, module);
            modules.Add(module);
        }
    }

    public static void CheckModules()
    {
        foreach (var module in modules.Where(module => module.IsValid && module.ShouldEnable != module.IsEnabled))
            ToggleOrInvalidateModule(module, true);
    }

    public static void ToggleOrInvalidateModule(Module module, bool logInfo)
    {
        try
        {
            module.Toggle();
            if (logInfo)
                PluginLog.Information(module.IsEnabled ? $"Enabled module: {module}" : $"Disabled module: {module}");
        }
        catch (Exception e)
        {
            PluginLog.Error(e, $"Error in module: {module}");
            module.IsValid = false;
        }
    }

    public static void Dispose()
    {
        foreach (var module in modules.Where(module => module.IsValid && module.IsEnabled))
            module.Toggle();
    }
}