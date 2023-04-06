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
using Dalamud.IoC;
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

    public DalamudApi() { }

    public DalamudApi(DalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
        if (!pluginInterface.Inject(this))
            throw new ApplicationException("Failed loading DalamudApi!");
    }

    public static DalamudApi operator +(DalamudApi container, object o)
    {
        foreach (var f in typeof(DalamudApi).GetProperties())
        {
            if (f.PropertyType != o.GetType()) continue;
            if (f.GetValue(container) != null) break;
            f.SetValue(container, o);
            return container;
        }
        throw new InvalidOperationException();
    }

    public static void Initialize(DalamudPluginInterface pluginInterface) => _ = new DalamudApi(pluginInterface);

    public static void Dispose() => SigScanner?.Dispose();
}