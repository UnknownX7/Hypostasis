using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;

namespace Hypostasis;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field), Conditional("DEBUG")]
public sealed class HypostasisDebugAttribute : Attribute { }

public sealed class HypostasisMemberDebugInfo
{
    public enum SignatureType
    {
        None,
        Scan,
        Text,
        Static,
        Pointer,
        Primitive,
        Hook,
        AsmHook,
        AsmPatch,
        GameFunction
    }

    public Util.AssignableInfo AssignableInfo { get; set; }
    public HypostasisMemberInjectionAttribute InjectionAttribute { get; set; }
    public string Signature { get; set; }
    public int Offset { get; set; }

    private nint address;
    public nint Address
    {
        get
        {
            if (address != nint.Zero || !string.IsNullOrEmpty(Signature)) return address;
            return address = Util.ConvertObjectToIntPtr(AssignableInfo?.GetValue());
        }
        set => address = value;
    }

    public SignatureType SigType { get; set; }
}

public static class Debug
{
    public const string HypostasisTag = "_HYPOSTASISPLUGINS";
    public static List<HypostasisMemberDebugInfo> SignatureInfos { get; } = new();
    public static Dictionary<int, (object, MemberInfo)> MemberInfos { get; } = new();
    public static ICallGateProvider<IDalamudPlugin> GetPluginProvider { get; private set; }
    public static ICallGateProvider<List<HypostasisMemberDebugInfo>> GetSigInfosProvider { get; private set; }
    public static ICallGateProvider<Dictionary<int, (object, MemberInfo)>> GetMemberInfosProvider { get; private set; }

    [Conditional("DEBUG")]
    public static void Initialize(IDalamudPlugin plugin)
    {
        var name = plugin.Name;
        GetPluginProvider = DalamudApi.PluginInterface.GetIpcProvider<IDalamudPlugin>($"{name}.{nameof(Hypostasis)}.GetPlugin");
        GetPluginProvider.RegisterFunc(() => plugin);
        GetSigInfosProvider = DalamudApi.PluginInterface.GetIpcProvider<List<HypostasisMemberDebugInfo>>($"{name}.{nameof(Hypostasis)}.GetSigInfos");
        GetSigInfosProvider.RegisterFunc(() => SignatureInfos);
        GetMemberInfosProvider = DalamudApi.PluginInterface.GetIpcProvider<Dictionary<int, (object, MemberInfo)>>($"{name}.{nameof(Hypostasis)}.GetMemberInfos");
        GetMemberInfosProvider.RegisterFunc(() => MemberInfos);
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

    /*public void AddMember(object o, MemberInfo memberInfo)
    {
        if (MemberInfos.Any(kv => kv.Value.Item2 == memberInfo)) return;

        var assignableInfo = new Util.AssignableInfo(o, memberInfo);
        var type = assignableInfo.Type;
        if (type == typeof(nint) || type.IsPointer || type.IsAssignableTo(typeof(Delegate)))
        {
            var address = Util.ConvertObjectToIntPtr(assignableInfo.GetValue());
            MemberInfos.Add(SignatureInfos.Count, (o, memberInfo));
            SignatureInfos.Add(new() { SigType = SignatureInfo.SignatureType.Pointer, Address = address });
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Hook<>) && assignableInfo.GetValue() is IDalamudHook hook)
        {
            MemberInfos.Add(SignatureInfos.Count, (o, memberInfo));
            SignatureInfos.Add(new() { SigType = SignatureInfo.SignatureType.Hook, Address = hook.Address });
        }
        else
        {
            switch (assignableInfo.GetValue())
            {
                case AsmPatch patch:
                    MemberInfos.Add(SignatureInfos.Count, (o, memberInfo));
                    SignatureInfos.Add(new() { SigType = SignatureInfo.SignatureType.AsmPatch, Signature = patch.Signature, Address = patch.Address });
                    break;
                case IGameFunction gameFunction:
                    MemberInfos.Add(SignatureInfos.Count, (o, memberInfo));
                    SignatureInfos.Add(new() { SigType = SignatureInfo.SignatureType.GameFunction, Signature = gameFunction.Signature, Address = gameFunction.Address });
                    break;
            }
        }
    }

    public void AddMember(Type type, object o, string member) => AddMember(o, type.GetMember(member, defaultBindingFlags)[0]);*/

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