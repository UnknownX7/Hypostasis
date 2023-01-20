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
    public class SigInfo
    {
        public enum SigType
        {
            None,
            Text,
            Static,
            Pointer,
            Primitive,
            Hook,
            AsmHook
        }

        public Util.AssignableInfo assignableInfo = null;
        public SignatureAttribute attribute = null;
        public SignatureExAttribute exAttribute = null;
        public string signature = string.Empty;
        public int offset = 0;
        public nint address = nint.Zero;
        public SigType sigType = SigType.None;
    }

    private const BindingFlags defaultBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    private readonly SigScanner sigScanner;
    private readonly Dictionary<string, nint> sigCache = new();
    private readonly Dictionary<string, nint> staticSigCache = new();
    private readonly List<IDisposable> disposableHooks = new();

    public ProcessModule Module => sigScanner.Module;
    public nint BaseAddress => Module.BaseAddress;
    public List<SigInfo> SigInfos { get; } = new();

    public SigScannerWrapper(SigScanner s) => sigScanner = s;

    public nint ScanText(string signature)
    {
        if (sigCache.TryGetValue(signature, out var ptr))
            return ptr;

        ptr = sigScanner.ScanText(signature);
        AddSignatureInfo(signature, ptr, 0, SigInfo.SigType.Text);
        return ptr;
    }

    public bool TryScanText(string signature, out nint result)
    {
        if (sigCache.TryGetValue(signature, out result))
            return true;

        var b = sigScanner.TryScanText(signature, out result);
        AddSignatureInfo(signature, result, 0, SigInfo.SigType.Text);
        return b;
    }

    public nint ScanData(string signature)
    {
        if (sigCache.TryGetValue(signature, out var ptr))
            return ptr;

        ptr = sigScanner.ScanData(signature);
        AddSignatureInfo(signature, ptr, 0, SigInfo.SigType.Text);
        return ptr;
    }

    public bool TryScanData(string signature, out nint result)
    {
        if (sigCache.TryGetValue(signature, out result))
            return true;

        var b = sigScanner.TryScanData(signature, out result);
        AddSignatureInfo(signature, result, 0, SigInfo.SigType.Text);
        return b;
    }

    public nint ScanModule(string signature)
    {
        if (sigCache.TryGetValue(signature, out var ptr))
            return ptr;

        ptr = sigScanner.ScanModule(signature);
        AddSignatureInfo(signature, ptr, 0, SigInfo.SigType.Text);
        return ptr;
    }

    public bool TryScanModule(string signature, out nint result)
    {
        if (sigCache.TryGetValue(signature, out result))
            return true;

        var b = sigScanner.TryScanModule(signature, out result);
        AddSignatureInfo(signature, result, 0, SigInfo.SigType.Text);
        return b;
    }

    public nint ScanStaticAddress(string signature, int offset = 0)
    {
        if (offset == 0 && staticSigCache.TryGetValue(signature, out var ptr))
            return ptr;

        ptr = sigScanner.GetStaticAddressFromSig(signature, offset);
        AddSignatureInfo(signature, ptr, offset, SigInfo.SigType.Static);
        return ptr;
    }

    public bool TryScanStaticAddress(string signature, out nint result, int offset = 0)
    {
        if (offset == 0 && staticSigCache.TryGetValue(signature, out result))
            return true;

        var b = sigScanner.TryGetStaticAddressFromSig(signature, out result, offset);
        AddSignatureInfo(signature, result, offset, SigInfo.SigType.Static);
        return b;
    }

    private void AddSignatureInfo(string signature, nint ptr, int offset, SigInfo.SigType type)
    {
        switch (type)
        {
            case SigInfo.SigType.Text when offset == 0:
                sigCache[signature] = ptr;
                break;
            case SigInfo.SigType.Static when offset == 0:
                staticSigCache[signature] = ptr;
                break;
        }

        SigInfos.Add(new SigInfo
        {
            signature = signature,
            offset = offset,
            address = ptr,
            sigType = type
        });
    }

    private Hook<T> HookAddress<T>(nint address, T detour, bool startEnabled = true, bool autoDispose = true, bool useMinHook = false) where T : Delegate
    {
        var hook = Hook<T>.FromAddress(address, detour, useMinHook);
        AddSignatureInfo(string.Empty, address, 0, SigInfo.SigType.Hook);

        if (startEnabled)
            hook.Enable();

        if (autoDispose)
            disposableHooks.Add(hook);

        return hook;
    }

    private Hook<T> HookSignature<T>(string signature, T detour, bool scanModule = false, bool startEnabled = true, bool autoDispose = true, bool useMinHook = false) where T : Delegate
    {
        var address = !scanModule ? sigScanner.ScanText(signature) : sigScanner.ScanModule(signature);
        var hook = Hook<T>.FromAddress(address, detour, useMinHook);
        AddSignatureInfo(signature, address, 0, SigInfo.SigType.Hook);

        if (startEnabled)
            hook.Enable();

        if (autoDispose)
            disposableHooks.Add(hook);

        return hook;
    }

    public void Inject(object o) => Inject(o.GetType(), o);

    public void Inject(Type type, object o = null)
    {
        foreach (var memberInfo in type.GetFields(defaultBindingFlags).Concat<MemberInfo>(type.GetProperties(defaultBindingFlags)))
            InjectMember(o, memberInfo);
    }

    private void InjectMember(object o, MemberInfo memberInfo)
    {
        var sigAttribute = memberInfo.GetCustomAttribute<SignatureAttribute>();
        if (sigAttribute == null) return;

        var exAttribute = memberInfo.GetCustomAttribute<SignatureExAttribute>() ?? new();
        var assignableInfo = new Util.AssignableInfo(o, memberInfo);
        var type = assignableInfo.Type;
        var name = assignableInfo.Name;
        var throwOnFail = sigAttribute.Fallibility == Fallibility.Infallible;
        var signature = sigAttribute.Signature;

        var sigInfo = new SigInfo { assignableInfo = assignableInfo, attribute = sigAttribute, exAttribute = exAttribute, signature = signature };
        SigInfos.Add(sigInfo);

        if (sigAttribute.ScanType == ScanType.Text ? !sigScanner.TryScanText(signature, out var ptr) : !sigScanner.TryGetStaticAddressFromSig(signature, out ptr))
        {
            LogSignatureAttributeError(type, name, $"Failed to find {sigAttribute.Signature} ({sigAttribute.ScanType}) signature", throwOnFail);
            return;
        }

        sigInfo.address = ptr;

        switch (sigAttribute.UseFlags)
        {
            case SignatureUseFlags.Auto when type == typeof(nint) || type.IsPointer || type.IsAssignableTo(typeof(Delegate)):
            case SignatureUseFlags.Pointer:
                sigInfo.sigType = SigInfo.SigType.Pointer;
                if (type.IsAssignableTo(typeof(Delegate)))
                    assignableInfo.SetValue(Marshal.GetDelegateForFunctionPointer(ptr, type));
                else
                    assignableInfo.SetValue(ptr);
                break;
            case SignatureUseFlags.Auto when type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Hook<>):
            case SignatureUseFlags.Hook:
                sigInfo.sigType = SigInfo.SigType.Hook;
                if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Hook<>))
                {
                    LogSignatureAttributeError(type, name, $"{type.Name} is not a Hook", throwOnFail);
                    return;
                }

                var hookDelegateType = type.GenericTypeArguments[0];
                var detourMethod = sigAttribute.DetourName == null ? type.GetMethod(name.Replace("Hook", "Detour"), defaultBindingFlags) : null;
                var detour = detourMethod != null
                    ? detourMethod.IsStatic ? Delegate.CreateDelegate(hookDelegateType, detourMethod, false) : Delegate.CreateDelegate(hookDelegateType, o, detourMethod, false)
                    : null;

                if (detour == null)
                {
                    if (sigAttribute.DetourName != null)
                    {
                        var method = type.GetMethod(sigAttribute.DetourName, defaultBindingFlags);
                        if (method == null)
                        {
                            LogSignatureAttributeError(type, name, $"Could not find detour \"{sigAttribute.DetourName}\"", throwOnFail);
                            return;
                        }

                        var del = method.IsStatic ? Delegate.CreateDelegate(hookDelegateType, method, false) : Delegate.CreateDelegate(hookDelegateType, o, method, false);
                        if (del == null)
                        {
                            LogSignatureAttributeError(type, name, $"Method {sigAttribute.DetourName} was not compatible with delegate {hookDelegateType.Name}", throwOnFail);
                            return;
                        }

                        detour = del;
                    }
                    else
                    {
                        var matches = type.GetMethods(defaultBindingFlags)
                            .Select(method => method.IsStatic ? Delegate.CreateDelegate(hookDelegateType, method, false) : Delegate.CreateDelegate(hookDelegateType, o, method, false))
                            .Where(del => del != null)
                            .ToArray();

                        if (matches.Length != 1)
                        {
                            LogSignatureAttributeError(type, name, "Either found no matching detours or found more than one: specify a detour name", throwOnFail);
                            return;
                        }

                        detour = matches[0]!;
                    }
                }

                var ctor = type.GetConstructor(new[] { typeof(nint), hookDelegateType });
                if (ctor == null)
                {
                    LogSignatureAttributeError(type, name, "Could not find Hook constructor", throwOnFail);
                    return;
                }

                var hook = ctor.Invoke(new object[] { ptr, detour });
                assignableInfo.SetValue(hook);

                if (exAttribute.EnableHook == EnableHook.Auto)
                    type.GetMethod("Enable")?.Invoke(hook, null);

                if (exAttribute.DisposeHook == DisposeHook.Auto)
                    disposableHooks.Add(hook as IDisposable);
                break;
            case SignatureUseFlags.Auto when type.IsPrimitive:
            case SignatureUseFlags.Offset:
                sigInfo.sigType = SigInfo.SigType.Primitive;
                var offset = Marshal.PtrToStructure(ptr + sigAttribute.Offset, type);
                assignableInfo.SetValue(offset);
                break;
            default:
                LogSignatureAttributeError(type, name, "Unable to detect SignatureUseFlags", throwOnFail);
                return;
        }
    }

    private static void LogSignatureAttributeError(Type classType, string memberName, string message, bool doThrow)
    {
        var errorMsg = $"Signature attribute error in {classType.FullName}.{memberName}: {message}";

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