using System;
using Dalamud.Game.Gui.Toast;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Hypostasis.Dalamud;

public class DalamudApi
{
    /// <inheritdoc cref="IDalamudPluginInterface"/>
    public static IDalamudPluginInterface PluginInterface { get; private set; }

    /// <inheritdoc cref="IAddonEventManager"/>
    [PluginService]
    public static IAddonEventManager AddonEventManager { get; private set; }

    /// <inheritdoc cref="IAddonLifecycle"/>
    [PluginService]
    public static IAddonLifecycle AddonLifecycle { get; private set; }

    /// <inheritdoc cref="IAetheryteList"/>
    [PluginService]
    public static IAetheryteList AetheryteList { get; private set; }

    /// <inheritdoc cref="IBuddyList"/>
    [PluginService]
    public static IBuddyList BuddyList { get; private set; }

    /// <inheritdoc cref="IChatGui"/>
    [PluginService]
    public static IChatGui ChatGui { get; private set; }

    /// <inheritdoc cref="IClientState"/>
    [PluginService]
    public static IClientState ClientState { get; private set; }

    /// <inheritdoc cref="ICommandManager"/>
    [PluginService]
    public static ICommandManager CommandManager { get; private set; }

    /// <inheritdoc cref="ICondition"/>
    [PluginService]
    public static ICondition Condition { get; private set; }

    //[PluginService]
    //public static IConsole Console { get; private set; }

    /// <inheritdoc cref="IContextMenu"/>
    [PluginService]
    public static IContextMenu ContextMenu { get; private set; }

    /// <inheritdoc cref="IDataManager"/>
    [PluginService]
    public static IDataManager DataManager { get; private set; }

    /// <inheritdoc cref="IDtrBar"/>
    [PluginService]
    public static IDtrBar DtrBar { get; private set; }

    /// <inheritdoc cref="IDutyState"/>
    [PluginService]
    public static IDutyState DutyState { get; private set; }

    /// <inheritdoc cref="IFateTable"/>
    [PluginService]
    public static IFateTable FateTable { get; private set; }

    /// <inheritdoc cref="IFlyTextGui"/>
    [PluginService]
    public static IFlyTextGui FlyTextGui { get; private set; }

    /// <inheritdoc cref="IFramework"/>
    [PluginService]
    public static IFramework Framework { get; private set; }

    /// <inheritdoc cref="IGameConfig"/>
    [PluginService]
    public static IGameConfig GameConfig { get; private set; }

    /// <inheritdoc cref="IGameGui"/>
    [PluginService]
    public static IGameGui GameGui { get; private set; }

    /// <inheritdoc cref="IGameInteropProvider"/>
    [PluginService]
    public static IGameInteropProvider GameInteropProvider { get; private set; }

    /// <inheritdoc cref="IGameInventory"/>
    [PluginService]
    public static IGameInventory GameInventory { get; private set; }

    /// <inheritdoc cref="IGameLifecycle"/>
    [PluginService]
    public static IGameLifecycle GameLifecycle { get; private set; }

    /// <inheritdoc cref="IGamepadState"/>
    [PluginService]
    public static IGamepadState GamepadState { get; private set; }

    /// <inheritdoc cref="IJobGauges"/>
    [PluginService]
    public static IJobGauges JobGauges { get; private set; }

    /// <inheritdoc cref="IKeyState"/>
    [PluginService]
    public static IKeyState KeyState { get; private set; }

    /// <inheritdoc cref="IMarketBoard"/>
    [PluginService]
    public static IMarketBoard MarketBoard { get; private set; }

    /// <inheritdoc cref="INamePlateGui"/>
    [PluginService]
    public static INamePlateGui NamePlateGui { get; private set; }

    /// <inheritdoc cref="INotificationManager"/>
    [PluginService]
    public static INotificationManager NotificationManager { get; private set; }

    /// <inheritdoc cref="IObjectTable"/>
    [PluginService]
    public static IObjectTable ObjectTable { get; private set; }

    /// <inheritdoc cref="IPartyFinderGui"/>
    [PluginService]
    public static IPartyFinderGui PartyFinderGui { get; private set; }

    /// <inheritdoc cref="IPartyList"/>
    [PluginService]
    public static IPartyList PartyList { get; private set; }

    /// <inheritdoc cref="IPlayerState"/>
    [PluginService]
    public static IPlayerState PlayerState { get; private set; }

    //[PluginService]
    //public static IPluginLinkHandler PluginLinkHandler { get; private set; }

    /// <inheritdoc cref="IPluginLog"/>
    [PluginService]
    public static IPluginLog PluginLog { get; private set; }

    //[PluginService]
    //public static IReliableFileStorage ReliableFileStorage { get; private set; }

    /// <inheritdoc cref="ISeStringEvaluator"/>
    [PluginService]
    public static ISeStringEvaluator SeStringEvaluator { get; private set; }

    /// <inheritdoc cref="ISelfTestRegistry"/>
    [PluginService]
    public static ISelfTestRegistry SelfTestRegistry { get; private set; }

    /// <inheritdoc cref="ISigScanner"/>
    [PluginService]
    private static ISigScanner sigScanner
    {
        set => SigScanner = new(value);
    }

    /// <inheritdoc cref="ISigScanner"/>
    public static SigScannerWrapper SigScanner { get; private set; }

    /// <inheritdoc cref="ITargetManager"/>
    [PluginService]
    public static ITargetManager TargetManager { get; private set; }

    /// <inheritdoc cref="ITextureProvider"/>
    [PluginService]
    public static ITextureProvider TextureProvider { get; private set; }

    /// <inheritdoc cref="ITextureReadbackProvider"/>
    [PluginService]
    public static ITextureReadbackProvider TextureReadbackProvider { get; private set; }

    /// <inheritdoc cref="ITextureSubstitutionProvider"/>
    [PluginService]
    public static ITextureSubstitutionProvider TextureSubstitutionProvider { get; private set; }

    /// <inheritdoc cref="ITitleScreenMenu"/>
    [PluginService]
    public static ITitleScreenMenu TitleScreenMenu { get; private set; }

    /// <inheritdoc cref="IToastGui"/>
    [PluginService]
    public static IToastGui ToastGui { get; private set; }

    //[PluginService]
    //public static IUnlockState UnlockState { get; private set; }

    private static readonly string printName = Hypostasis.PluginName;
    private static readonly string printHeader = $"[{printName}] ";

    public DalamudApi(IDalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
        if (!pluginInterface.Inject(this))
            throw new ApplicationException("Failed loading DalamudApi!");
    }

    public static void PrintEcho(string message) => ChatGui.Print($"{printHeader}{message}");

    public static void PrintError(string message) => ChatGui.PrintError($"{printHeader}{message}");

    public static void ShowNotification(string message, NotificationType type = NotificationType.None, uint msDelay = 3_000u) => NotificationManager.AddNotification(new Notification { Type = type, Title = printName, Content = message, InitialDuration = TimeSpan.FromMilliseconds(msDelay) });

    public static void ShowToast(string message, ToastOptions options = null) => ToastGui.ShowNormal($"{printHeader}{message}", options);

    public static void ShowQuestToast(string message, QuestToastOptions options = null) => ToastGui.ShowQuest($"{printHeader}{message}", options);

    public static void ShowErrorToast(string message) => ToastGui.ShowError($"{printHeader}{message}");

    public static void LogVerbose(string message, Exception exception = null) => PluginLog.Verbose(exception, message);

    public static void LogDebug(string message, Exception exception = null) => PluginLog.Debug(exception, message);

    public static void LogInfo(string message, Exception exception = null) => PluginLog.Information(exception, message);

    public static void LogWarning(string message, Exception exception = null) => PluginLog.Warning(exception, message);

    public static void LogError(string message, Exception exception = null) => PluginLog.Error(exception, message);

    public static void LogFatal(string message, Exception exception = null) => PluginLog.Fatal(exception, message);

    public static void Initialize(IDalamudPluginInterface pluginInterface) => _ = new DalamudApi(pluginInterface);

    public static void Dispose() => SigScanner?.Dispose();
}