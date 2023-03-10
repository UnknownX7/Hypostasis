using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace Hypostasis;

public static class Debug
{
    public const string HypostasisTag = "_HYPOSTASISPLUGINS";
    public static ICallGateProvider<IDalamudPlugin> GetPluginProvider { get; private set; }
    public static ICallGateProvider<List<SigScannerWrapper.SignatureInfo>> GetSigInfosProvider { get; private set; }
    public static ICallGateProvider<Dictionary<int, (object, MemberInfo)>> GetMemberInfosProvider { get; private set; }

    [Conditional("DEBUG")]
    public static void Initialize(IDalamudPlugin plugin)
    {
        var name = plugin.Name;
        GetPluginProvider = DalamudApi.PluginInterface.GetIpcProvider<IDalamudPlugin>($"{name}.{nameof(Hypostasis)}.GetPlugin");
        GetPluginProvider.RegisterFunc(() => plugin);
        GetSigInfosProvider = DalamudApi.PluginInterface.GetIpcProvider<List<SigScannerWrapper.SignatureInfo>>($"{name}.{nameof(Hypostasis)}.GetSigInfos");
        GetSigInfosProvider.RegisterFunc(() => DalamudApi.SigScanner.SignatureInfos);
        GetMemberInfosProvider = DalamudApi.PluginInterface.GetIpcProvider<Dictionary<int, (object, MemberInfo)>>($"{name}.{nameof(Hypostasis)}.GetMemberInfos");
        GetMemberInfosProvider.RegisterFunc(() => DalamudApi.SigScanner.MemberInfos);
        DalamudApi.Framework.RunOnTick(EnableDebugging);
    }

    private static void EnableDebugging()
    {
        var plugins = DalamudApi.PluginInterface.GetOrCreateData(HypostasisTag, () => new HashSet<string>());
        lock (plugins)
            plugins.Add(Hypostasis.PluginName);
    }

    private static void DisableDebugging()
    {
        if (!DalamudApi.PluginInterface.TryGetData<HashSet<string>>(HypostasisTag, out var plugins)) return;
        lock (plugins)
            plugins.Remove(Hypostasis.PluginName);
    }

    [Conditional("DEBUG")]
    public static void Dispose()
    {
        DisableDebugging();
        DalamudApi.PluginInterface.RelinquishData(HypostasisTag);
        GetPluginProvider?.UnregisterFunc();
        GetSigInfosProvider?.UnregisterFunc();
        GetMemberInfosProvider?.UnregisterFunc();
    }
}