using System;
using System.Reflection;
using Dalamud.Configuration;
using Dalamud.Game;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace Hypostasis;

public abstract class DalamudPlugin<P, C> where P : DalamudPlugin<P, C>, IDalamudPlugin where C : PluginConfiguration<C>, IPluginConfiguration, new()
{
    private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    public abstract string Name { get; }
    public static P Plugin { get; private set; }
    public static C Config { get; private set; }

    private static string printHeader;
    private readonly bool addedUpdate, addedDraw, addedConfig;
    private readonly PluginCommandManager pluginCommandManager;

    public DalamudPlugin(DalamudPluginInterface pluginInterface)
    {
        Plugin = this as P;
        printHeader = $"[{Name}] ";

        Hypostasis.Initialize(pluginInterface);

        Config = PluginConfiguration<C>.LoadConfig();
        Config.Initialize();

        pluginCommandManager = new(Plugin);

        try
        {
            var derivedType = typeof(P);

            if (derivedType.GetMethod("Update", bindingFlags, new Type[] { typeof(Framework) })?.DeclaringType == derivedType)
            {
                DalamudApi.Framework.Update += Update;
                addedUpdate = true;
            }

            if (derivedType.GetMethod("Draw", bindingFlags, Type.EmptyTypes)?.DeclaringType == derivedType)
            {
                DalamudApi.PluginInterface.UiBuilder.Draw += Draw;
                addedDraw = true;
            }

            if (derivedType.GetMethod("ToggleConfig", bindingFlags, Type.EmptyTypes)?.DeclaringType == derivedType)
            {
                DalamudApi.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
                addedConfig = true;
            }
        }
        catch (Exception e)
        {
            PluginLog.Error($"Failed loading {Name}\n{e}");
        }
    }

    public static void PrintEcho(string message) => DalamudApi.ChatGui.Print(printHeader + message);

    public static void PrintError(string message) => DalamudApi.ChatGui.PrintError(printHeader + message);

    protected virtual void ToggleConfig() { }

    protected virtual void Update(Framework framework) { }

    protected virtual void Draw() { }

    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        Config.Save();

        if (addedUpdate)
            DalamudApi.Framework.Update -= Update;

        if (addedDraw)
            DalamudApi.PluginInterface.UiBuilder.Draw -= Draw;

        if (addedConfig)
            DalamudApi.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;

        pluginCommandManager.Dispose();
        Hypostasis.Dispose();

        GC.SuppressFinalize(this);
    }
}