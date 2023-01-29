using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Hypostasis.Game;

public class SigScannerWrapper : IDisposable
{
    public class SignatureInfo
    {
        public enum SignatureType
        {
            None,
            Text,
            Static,
            Pointer,
            Primitive,
            Hook,
            AsmHook
        }

        public Util.AssignableInfo AssignableInfo { get; set; }
        public SignatureAttribute SigAttribute { get; set; }
        public SignatureExAttribute ExAttribute { get; set; }
        public ClientStructsAttribute CSAttribute { get; set; }
        public string Signature { get; set; } = string.Empty;
        public int Offset { get; set; }

        private nint address;
        public unsafe nint Address
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

    private const BindingFlags defaultBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private readonly SigScanner sigScanner;
    private readonly Dictionary<string, nint> sigCache = new();
    private readonly Dictionary<string, nint> staticSigCache = new();
    private readonly List<IDisposable> disposableHooks = new();

    public ProcessModule Module => sigScanner.Module;
    public nint BaseAddress => Module.BaseAddress;
    public nint BaseTextAddress => (nint)(BaseAddress + sigScanner.TextSectionOffset);
    public nint BaseDataAddress => (nint)(BaseAddress + sigScanner.DataSectionOffset);
    public nint BaseRDataAddress => (nint)(BaseAddress + sigScanner.RDataSectionOffset);
    public List<SignatureInfo> SignatureInfos { get; } = new();
    public Dictionary<int, (object, MemberInfo)> MemberInfos { get; } = new();

    public SigScannerWrapper(SigScanner s) => sigScanner = s;

    public nint ScanText(string signature)
    {
        if (sigCache.TryGetValue(signature, out var ptr))
            return ptr;

        ptr = sigScanner.ScanText(signature);
        AddSignatureInfo(signature, ptr, 0, SignatureInfo.SignatureType.Text);
        return ptr;
    }

    public bool TryScanText(string signature, out nint result)
    {
        if (sigCache.TryGetValue(signature, out result))
            return true;

        var b = sigScanner.TryScanText(signature, out result);
        AddSignatureInfo(signature, result, 0, SignatureInfo.SignatureType.Text);
        return b;
    }

    public nint ScanData(string signature)
    {
        if (sigCache.TryGetValue(signature, out var ptr))
            return ptr;

        ptr = sigScanner.ScanData(signature);
        AddSignatureInfo(signature, ptr, 0, SignatureInfo.SignatureType.Text);
        return ptr;
    }

    public bool TryScanData(string signature, out nint result)
    {
        if (sigCache.TryGetValue(signature, out result))
            return true;

        var b = sigScanner.TryScanData(signature, out result);
        AddSignatureInfo(signature, result, 0, SignatureInfo.SignatureType.Text);
        return b;
    }

    public nint ScanModule(string signature)
    {
        if (sigCache.TryGetValue(signature, out var ptr))
            return ptr;

        ptr = sigScanner.ScanModule(signature);
        AddSignatureInfo(signature, ptr, 0, SignatureInfo.SignatureType.Text);
        return ptr;
    }

    public bool TryScanModule(string signature, out nint result)
    {
        if (sigCache.TryGetValue(signature, out result))
            return true;

        var b = sigScanner.TryScanModule(signature, out result);
        AddSignatureInfo(signature, result, 0, SignatureInfo.SignatureType.Text);
        return b;
    }

    public nint ScanStaticAddress(string signature, int offset = 0)
    {
        if (offset == 0 && staticSigCache.TryGetValue(signature, out var ptr))
            return ptr;

        ptr = sigScanner.GetStaticAddressFromSig(signature, offset);
        AddSignatureInfo(signature, ptr, offset, SignatureInfo.SignatureType.Static);
        return ptr;
    }

    public bool TryScanStaticAddress(string signature, out nint result, int offset = 0)
    {
        if (offset == 0 && staticSigCache.TryGetValue(signature, out result))
            return true;

        var b = sigScanner.TryGetStaticAddressFromSig(signature, out result, offset);
        AddSignatureInfo(signature, result, offset, SignatureInfo.SignatureType.Static);
        return b;
    }

    private void AddSignatureInfo(string signature, nint ptr, int offset, SignatureInfo.SignatureType type)
    {
        switch (type)
        {
            case SignatureInfo.SignatureType.Text when offset == 0:
                sigCache[signature] = ptr;
                break;
            case SignatureInfo.SignatureType.Static when offset == 0:
                staticSigCache[signature] = ptr;
                break;
        }

        var sigInfo = new SignatureInfo
        {
            Signature = signature,
            Offset = offset,
            Address = ptr,
            SigType = type
        };
        SignatureInfos.Add(sigInfo);
    }

    private Hook<T> HookAddress<T>(nint address, T detour, bool startEnabled = true, bool autoDispose = true, bool useMinHook = false) where T : Delegate
    {
        var hook = Hook<T>.FromAddress(address, detour, useMinHook);
        AddSignatureInfo(string.Empty, address, 0, SignatureInfo.SignatureType.Hook);
        AddHook(hook, startEnabled, autoDispose);
        return hook;
    }

    private Hook<T> HookSignature<T>(string signature, T detour, bool scanModule = false, bool startEnabled = true, bool autoDispose = true, bool useMinHook = false) where T : Delegate
    {
        var address = !scanModule ? sigScanner.ScanText(signature) : sigScanner.ScanModule(signature);
        var hook = Hook<T>.FromAddress(address, detour, useMinHook);
        AddSignatureInfo(signature, address, 0, SignatureInfo.SignatureType.Hook);
        AddHook(hook, startEnabled, autoDispose);
        return hook;
    }

    public void Inject(Type type, object o = null, bool addAllMembers = true)
    {
        foreach (var memberInfo in type.GetFields(defaultBindingFlags).Concat<MemberInfo>(type.GetProperties(defaultBindingFlags)))
            InjectMember(o, memberInfo, addAllMembers);
    }

    public void Inject(object o, bool addAllMembers = true) => Inject(o.GetType(), o, addAllMembers);

    public void InjectMember(object o, MemberInfo memberInfo, bool addAllMembers = true)
    {
        if (memberInfo.GetCustomAttribute<ClientStructsAttribute>() is { } csAttribute)
            InjectMember(o, memberInfo, csAttribute);
        else if (memberInfo.GetCustomAttribute<SignatureAttribute>() is { } sigAttribute)
            InjectMember(o, memberInfo, sigAttribute);
        else if (addAllMembers)
            AddMember(o, memberInfo);
    }

    public void InjectMember(object o, MemberInfo memberInfo, SignatureAttribute sigAttribute)
    {
        var ownerType = memberInfo.ReflectedType;
        var exAttribute = memberInfo.GetCustomAttribute<SignatureExAttribute>() ?? new();
        var assignableInfo = new Util.AssignableInfo(o, memberInfo);
        var type = assignableInfo.Type;
        var name = assignableInfo.Name;

        if (ownerType == null)
        {
            LogSignatureAttributeError(null, name, "ReflectedType was null!", true);
            return;
        }

        var throwOnFail = sigAttribute.Fallibility == Fallibility.Infallible;
        var signature = sigAttribute.Signature;

        var sigInfo = new SignatureInfo { SigAttribute = sigAttribute, ExAttribute = exAttribute, Signature = signature };
        MemberInfos.Add(SignatureInfos.Count, (o, memberInfo));
        SignatureInfos.Add(sigInfo);

        if (sigAttribute.ScanType == ScanType.Text ? !sigScanner.TryScanText(signature, out var ptr) : !sigScanner.TryGetStaticAddressFromSig(signature, out ptr))
        {
            LogSignatureAttributeError(ownerType, name, $"Failed to find {sigAttribute.Signature} ({sigAttribute.ScanType}) signature", throwOnFail);
            return;
        }

        sigInfo.Address = ptr;

        switch (sigAttribute.UseFlags)
        {
            case SignatureUseFlags.Auto when type == typeof(nint) || type.IsPointer || type.IsAssignableTo(typeof(Delegate)):
            case SignatureUseFlags.Pointer:
                sigInfo.SigType = SignatureInfo.SignatureType.Pointer;
                if (type.IsAssignableTo(typeof(Delegate)))
                    assignableInfo.SetValue(Marshal.GetDelegateForFunctionPointer(ptr, type));
                else
                    assignableInfo.SetValue(ptr);
                break;
            case SignatureUseFlags.Auto when type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Hook<>):
            case SignatureUseFlags.Hook:
                sigInfo.SigType = SignatureInfo.SignatureType.Hook;
                if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Hook<>))
                {
                    LogSignatureAttributeError(ownerType, name, $"{type.Name} is not a Hook", throwOnFail);
                    return;
                }
                InjectHook(type, assignableInfo, o, ptr, sigAttribute, exAttribute);
                break;
            case SignatureUseFlags.Auto when type.IsPrimitive:
            case SignatureUseFlags.Offset:
                sigInfo.SigType = SignatureInfo.SignatureType.Primitive;
                var offset = Marshal.PtrToStructure(ptr + sigAttribute.Offset, type);
                assignableInfo.SetValue(offset);
                break;
            default:
                LogSignatureAttributeError(ownerType, name, "Unable to detect SignatureUseFlags", throwOnFail);
                return;
        }
    }

    public unsafe void InjectMember(object o, MemberInfo memberInfo, ClientStructsAttribute csAttribute)
    {
        var memberName = memberInfo.Name.EndsWith("Hook") ? memberInfo.Name.Replace("Hook", string.Empty) : csAttribute.MemberName;
        var csMember = csAttribute.ClientStructsType.GetMember(memberName)[0];
        var sigAttribute = memberInfo.GetCustomAttribute<SignatureAttribute>() ?? new("");
        var exAttribute = memberInfo.GetCustomAttribute<SignatureExAttribute>() ?? new();
        var assignableInfo = new Util.AssignableInfo(o, memberInfo);
        var type = assignableInfo.Type;
        var sigInfo = new SignatureInfo { ExAttribute = exAttribute, CSAttribute = csAttribute };
        MemberInfos.Add(SignatureInfos.Count, (o, memberInfo));
        SignatureInfos.Add(sigInfo);

        var retrievedValue = csMember switch
        {
            FieldInfo f => f.GetValue(null),
            PropertyInfo p => p.GetValue(null),
            MethodInfo m => m.Invoke(null, Array.Empty<object>()),
            _ => throw new ApplicationException("Member type is unsupported")
        };

        sigInfo.Address = Util.ConvertObjectToIntPtr(retrievedValue);

        if (type == typeof(nint) || type.IsPointer || type.IsAssignableTo(typeof(Delegate)))
        {
            sigInfo.SigType = SignatureInfo.SignatureType.Pointer;
            assignableInfo.SetValue(sigInfo.Address);
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Hook<>))
        {
            sigInfo.SigType = SignatureInfo.SignatureType.Hook;
            InjectHook(type, assignableInfo, o, sigInfo.Address, sigAttribute, exAttribute);
        }
    }

    private void InjectHook(Type type, Util.AssignableInfo assignableInfo, object o, nint ptr, SignatureAttribute sigAttribute, SignatureExAttribute exAttribute)
    {
        var ownerType = assignableInfo.MemberInfo.DeclaringType;
        var hookDelegateType = type.GenericTypeArguments[0];
        var name = assignableInfo.Name;
        var throwOnFail = sigAttribute.Fallibility == Fallibility.Infallible;
        var detour = GetMethodDelegate(ownerType, hookDelegateType, o, name.Replace("Hook", "Detour"));

        if (detour == null)
        {
            var detourName = sigAttribute.DetourName;
            if (detourName != null)
            {
                detour = GetMethodDelegate(ownerType, hookDelegateType, o, detourName);
                if (detour == null)
                {
                    LogSignatureAttributeError(ownerType, name, $"Detour not found or was incompatible with delegate \"{detourName}\" {hookDelegateType.Name}", throwOnFail);
                    return;
                }
            }
            else
            {
                var matches = GetMethodDelegates(ownerType, hookDelegateType, o);
                if (matches.Length != 1)
                {
                    LogSignatureAttributeError(ownerType, name, $"Found {matches.Length} matching detours: specify a detour name", throwOnFail);
                    return;
                }

                detour = matches[0]!;
            }
        }

        var ctor = type.GetConstructor(new[] { typeof(nint), hookDelegateType });
        if (ctor == null)
        {
            LogSignatureAttributeError(ownerType, name, "Could not find Hook constructor", throwOnFail);
            return;
        }

        var hook = ctor.Invoke(new object[] { ptr, detour });
        assignableInfo.SetValue(hook);

        if (exAttribute.EnableHook)
            type.GetMethod("Enable")?.Invoke(hook, null);
        if (exAttribute.DisposeHook)
            disposableHooks.Add(hook as IDisposable);
    }

    private static Delegate GetMethodDelegate(IReflect ownerType, Type delegateType, object o, string methodName)
    {
        var detourMethod = ownerType.GetMethod(methodName, defaultBindingFlags);
        return CreateDelegate(delegateType, o, detourMethod);
    }

    private static Delegate[] GetMethodDelegates(IReflect ownerType, Type delegateType, object o) => ownerType.GetMethods(defaultBindingFlags)
        .Select(methodInfo => CreateDelegate(delegateType, o, methodInfo)).Where(del => del != null).ToArray();

    private static Delegate CreateDelegate(Type delegateType, object o, MethodInfo delegateMethod)
    {
        if (delegateType == null) return null;
        return delegateMethod.IsStatic
            ? Delegate.CreateDelegate(delegateType, delegateMethod, false)
            : Delegate.CreateDelegate(delegateType, o, delegateMethod, false);
    }

    public unsafe void AddMember(object o, MemberInfo memberInfo)
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
    }

    public void AddMember(Type type, object o, string member) => AddMember(o, type.GetMember(member, defaultBindingFlags)[0]);

    public void AddHook<T>(Hook<T> hook, bool enable = true, bool dispose = true) where T : Delegate
    {
        if (enable)
            hook.Enable();
        if (dispose)
            disposableHooks.Add(hook);
    }

    public void InjectMember(Type type, object o, string member) => InjectMember(o, type.GetMember(member, defaultBindingFlags)[0]);

    private static void LogSignatureAttributeError(Type classType, string memberName, string message, bool doThrow)
    {
        var errorMsg = $"Signature attribute error in {classType?.FullName}.{memberName}:\n{message}";

        if (doThrow)
            throw new ApplicationException(errorMsg);

        PluginLog.Warning(errorMsg);
    }

    public void Dispose()
    {
        foreach (var hook in disposableHooks)
            hook?.Dispose();
        GC.SuppressFinalize(this);
    }
}