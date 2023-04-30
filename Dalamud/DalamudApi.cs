using System;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Aetherytes;
using Dalamud.Game.ClientState.Buddy;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.GamePad;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Config;
using Dalamud.Game.DutyState;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.PartyFinder;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Libc;
using Dalamud.Game.Network;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Hypostasis.Dalamud;

public class DalamudApi
{
    //[PluginService]
    public static DalamudPluginInterface PluginInterface { get; private set; }

    [PluginService]
    public static AetheryteList AetheryteList { get; private set; }

    [PluginService]
    public static BuddyList BuddyList { get; private set; }

    [PluginService]
    public static ChatGui ChatGui { get; private set; }

    [PluginService]
    public static ChatHandlers ChatHandlers { get; private set; }

    [PluginService]
    public static ClientState ClientState { get; private set; }

    [PluginService]
    public static CommandManager CommandManager { get; private set; }

    [PluginService]
    public static Condition Condition { get; private set; }

    [PluginService]
    public static DataManager DataManager { get; private set; }

    [PluginService]
    public static DtrBar DtrBar { get; private set; }

    [PluginService]
    public static DutyState DutyState { get; private set; }

    [PluginService]
    public static FateTable FateTable { get; private set; }

    [PluginService]
    public static FlyTextGui FlyTextGui { get; private set; }

    [PluginService]
    public static Framework Framework { get; private set; }

    [PluginService]
    public static GameConfig GameConfig { get; private set; }

    [PluginService]
    public static GameGui GameGui { get; private set; }

    [PluginService]
    public static GameLifecycle GameLifecycle { get; private set; }

    [PluginService]
    public static GameNetwork GameNetwork { get; private set; }

    [PluginService]
    public static GamepadState GamepadState { get; private set; }

    [PluginService]
    public static JobGauges JobGauges { get; private set; }

    [PluginService]
    public static KeyState KeyState { get; private set; }

    [PluginService]
    public static LibcFunction LibcFunction { get; private set; }

    [PluginService]
    public static ObjectTable ObjectTable { get; private set; }

    [PluginService]
    public static PartyFinderGui PartyFinderGui { get; private set; }

    [PluginService]
    public static PartyList PartyList { get; private set; }

    [PluginService]
    private static SigScanner sigScanner
    {
        set => SigScanner = new(value);
    }

    public static SigScannerWrapper SigScanner { get; private set; }

    [PluginService]
    public static TargetManager TargetManager { get; private set; }

    [PluginService]
    public static TitleScreenMenu TitleScreenMenu { get; private set; }

    [PluginService]
    public static ToastGui ToastGui { get; private set; }

    private static readonly string printName = Hypostasis.PluginName;
    private static readonly string printHeader = $"[{printName}] ";

    public DalamudApi(DalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
        if (!pluginInterface.Inject(this))
            throw new ApplicationException("Failed loading DalamudApi!");
    }

    public static void PrintEcho(string message) => ChatGui.Print($"{printHeader}{message}");

    public static void PrintError(string message) => ChatGui.PrintError($"{printHeader}{message}");

    public static void ShowNotification(string message, NotificationType type = NotificationType.None, uint msDelay = 3_000u) => PluginInterface.UiBuilder.AddNotification(message, printName, type, msDelay);

    public static void ShowToast(string message, ToastOptions options = null) => ToastGui.ShowNormal($"{printHeader}{message}", options);

    public static void ShowQuestToast(string message, QuestToastOptions options = null) => ToastGui.ShowQuest($"{printHeader}{message}", options);

    public static void ShowErrorToast(string message) => ToastGui.ShowError($"{printHeader}{message}");

    public static void LogVerbose(string message, Exception exception = null) => PluginLog.Verbose(exception, message);

    public static void LogDebug(string message, Exception exception = null) => PluginLog.Debug(exception, message);

    public static void LogInfo(string message, Exception exception = null) => PluginLog.Information(exception, message);

    public static void LogWarning(string message, Exception exception = null) => PluginLog.Warning(exception, message);

    public static void LogError(string message, Exception exception = null) => PluginLog.Error(exception, message);

    public static void LogFatal(string message, Exception exception = null) => PluginLog.Fatal(exception, message);

    public static void Initialize(DalamudPluginInterface pluginInterface) => _ = new DalamudApi(pluginInterface);

    public static void Dispose() => SigScanner?.Dispose();
}