using System.Collections.Generic;
using System.Reflection;
using Dalamud.Plugin.Ipc;

namespace Hypostasis;

public static class IPC
{
    public static ICallGateProvider<List<SigScannerWrapper.SigInfo>> GetSigInfosProvider { get; private set; }
    public static ICallGateProvider<Dictionary<int, (object, MemberInfo)>> GetMemberInfosProvider { get; private set; }

    public static void Initialize(string pluginName)
    {
        GetSigInfosProvider = DalamudApi.PluginInterface.GetIpcProvider<List<SigScannerWrapper.SigInfo>>($"{pluginName}.Hypostasis.GetSigInfos");
        GetSigInfosProvider.RegisterFunc(() => DalamudApi.SigScanner.SigInfos);
        GetMemberInfosProvider = DalamudApi.PluginInterface.GetIpcProvider<Dictionary<int, (object, MemberInfo)>>($"{pluginName}.Hypostasis.GetMemberInfos");
        GetMemberInfosProvider.RegisterFunc(() => DalamudApi.SigScanner.MemberInfos);
    }

    public static void Dispose()
    {
        GetSigInfosProvider?.UnregisterFunc();
        GetMemberInfosProvider?.UnregisterFunc();
    }
}