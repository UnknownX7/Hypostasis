using System;
using System.Diagnostics;
using System.Reflection;
using Dalamud.Configuration;
using Dalamud.Game;
using Dalamud.Game.Gui.Toast;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace Hypostasis.Dalamud;

public abstract class DalamudPlugin<P, C> where P : DalamudPlugin<P, C>, IDalamudPlugin where C : PluginConfiguration<C>, IPluginConfiguration, new()
{
    private const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    public abstract string Name { get; }
    public static P Plugin { get; private set; }
    public static C Config { get; private set; }

    private static string printName, printHeader;
    private readonly bool addedUpdate, addedDraw, addedConfig;
    private readonly PluginCommandManager pluginCommandManager;

    protected DalamudPlugin(DalamudPluginInterface pluginInterface)
    {
#if DEBUG
        var stopwatch = Stopwatch.StartNew();
#endif

        try
        {
            Plugin = this as P;
            printName = Name;
            printHeader = $"[{printName}] ";

            Hypostasis.Initialize(Plugin, pluginInterface);
            Config = PluginConfiguration<C>.LoadConfig();
            pluginCommandManager = new(Plugin);
        }
        catch (Exception e)
        {
            PluginLog.Error(e, $"Failed loading {nameof(Hypostasis)} for {printName}");
            Dispose();
            Hypostasis.State = Hypostasis.PluginState.Failed;
            return;
        }

#if DEBUG
        var hypostasisMS = stopwatch.Elapsed.TotalMilliseconds;
        stopwatch.Restart();
#endif

        try
        {
            DalamudApi.SigScanner.InjectSignatures();
            Initialize();

            if (!PluginModuleManager.Initialize())
                ShowNotification("One or more modules failed to load,\nplease check /xllog for more info", NotificationType.Warning, 10_000);

            var derivedType = typeof(P);

            if (derivedType.GetMethod(nameof(Update), bindingFlags, new[] { typeof(Framework) })?.DeclaringType == derivedType)
            {
                DalamudApi.Framework.Update += Update;
                addedUpdate = true;
            }

            if (derivedType.GetMethod(nameof(Draw), bindingFlags, Type.EmptyTypes)?.DeclaringType == derivedType)
            {
                DalamudApi.PluginInterface.UiBuilder.Draw += Draw;
                addedDraw = true;
            }

            if (derivedType.GetMethod(nameof(ToggleConfig), bindingFlags, Type.EmptyTypes)?.DeclaringType == derivedType)
            {
                DalamudApi.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfig;
                addedConfig = true;
            }

            Hypostasis.State = Hypostasis.PluginState.Loaded;

#if DEBUG
            ShowNotification($"{nameof(Hypostasis)} initialized in {hypostasisMS} ms\nPlugin initialized in {stopwatch.Elapsed.TotalMilliseconds} ms", NotificationType.Info);
#endif
        }
        catch (Exception e)
        {
            // Excessive? Yes.
            var msg = $"Failed loading {printName}";
            PluginLog.Error(e, msg);
            ShowNotification($"\t\t\t{msg}\t\t\t\n\n", NotificationType.Error, 10_000);
            ShowErrorToast(msg);
            PrintError(msg);
            Dispose();
            Hypostasis.State = Hypostasis.PluginState.Failed;
        }
    }

    public static void PrintEcho(string message) => DalamudApi.ChatGui.Print(printHeader + message);

    public static void PrintError(string message) => DalamudApi.ChatGui.PrintError(printHeader + message);

    public static void ShowNotification(string message, NotificationType type = NotificationType.None, uint msDelay = 3_000u) => DalamudApi.PluginInterface.UiBuilder.AddNotification(message, printName, type, msDelay);

    public static void ShowToast(string message, ToastOptions options = null) => DalamudApi.ToastGui.ShowNormal(printHeader + message, options);

    public static void ShowQuestToast(string message, QuestToastOptions options = null) => DalamudApi.ToastGui.ShowQuest(printHeader + message, options);

    public static void ShowErrorToast(string message) => DalamudApi.ToastGui.ShowError(printHeader + message);

    protected virtual void Initialize() { }

    protected virtual void ToggleConfig() { }

    protected virtual void Update(Framework framework) { }

    protected virtual void Draw() { }

    protected abstract void Dispose(bool disposing);

    public void Dispose()
    {
        var failed = Hypostasis.State == Hypostasis.PluginState.Loading;
        Hypostasis.State = Hypostasis.PluginState.Unloading;
        Config?.Save();

        if (addedUpdate)
            DalamudApi.Framework.Update -= Update;

        if (addedDraw)
            DalamudApi.PluginInterface.UiBuilder.Draw -= Draw;

        if (addedConfig)
            DalamudApi.PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfig;

        Dispose(true);

        pluginCommandManager?.Dispose();
        Hypostasis.Dispose(failed);

        Hypostasis.State = Hypostasis.PluginState.Unloaded;
        GC.SuppressFinalize(this);
    }
}