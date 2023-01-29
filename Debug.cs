#if DEBUG
using System.Collections.Generic;
using System.Reflection;
using Dalamud.Plugin.Ipc;

namespace Hypostasis;

public static class Debug
{
    public const string HypostasisTag = "_HYPOSTASISPLUGINS";
    public static ICallGateProvider<List<SigScannerWrapper.SignatureInfo>> GetSigInfosProvider { get; private set; }
    public static ICallGateProvider<Dictionary<int, (object, MemberInfo)>> GetMemberInfosProvider { get; private set; }

    public static void Initialize(string pluginName)
    {
        GetSigInfosProvider = DalamudApi.PluginInterface.GetIpcProvider<List<SigScannerWrapper.SignatureInfo>>($"{pluginName}.Hypostasis.GetSigInfos");
        GetSigInfosProvider.RegisterFunc(() => DalamudApi.SigScanner.SignatureInfos);
        GetMemberInfosProvider = DalamudApi.PluginInterface.GetIpcProvider<Dictionary<int, (object, MemberInfo)>>($"{pluginName}.Hypostasis.GetMemberInfos");
        GetMemberInfosProvider.RegisterFunc(() => DalamudApi.SigScanner.MemberInfos);
        var plugins = DalamudApi.PluginInterface.GetOrCreateData(HypostasisTag, () => new HashSet<string>());
        lock (plugins)
            plugins.Add(pluginName);
    }

    public static void Dispose()
    {
        if (DalamudApi.PluginInterface.TryGetData<HashSet<string>>(HypostasisTag, out var plugins))
            lock (plugins)
                plugins.Remove(Hypostasis.PluginName);

        DalamudApi.PluginInterface.RelinquishData(HypostasisTag);
        GetSigInfosProvider?.UnregisterFunc();
        GetMemberInfosProvider?.UnregisterFunc();
    }
}
#endif