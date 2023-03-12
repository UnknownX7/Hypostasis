using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Hooking;
using Dalamud.Logging;

namespace Hypostasis.Game;

[HypostasisDebuggable]
public sealed class AsmPatch : IDisposable
{
    public nint Address { get; } = nint.Zero;
    public string Signature { get; } = string.Empty;
    public byte[] NewBytes { get; }
    public byte[] OldBytes { get; }
    public bool IsEnabled { get; private set; } = false;
    public bool IsValid => Address != nint.Zero;
    public string ReadBytes => !IsValid ? string.Empty : OldBytes.Aggregate(string.Empty, (current, b) => current + b.ToString("X2") + " ");
    private readonly AsmHook hook = null;
    private static readonly List<AsmPatch> asmPatches = new();

    public AsmPatch(nint addr, byte[] bytes, bool startEnabled = false, bool useASMHook = false)
    {
        if (addr == nint.Zero) return;

        Address = addr;
        NewBytes = bytes;
        SafeMemory.ReadBytes(addr, bytes.Length, out var oldBytes);
        OldBytes = oldBytes;
        asmPatches.Add(this);

        if (useASMHook)
            hook = new(addr, NewBytes, $"{Util.AssemblyName.Name} AsmPatch#{asmPatches.Count}", AsmHookBehaviour.DoNotExecuteOriginal);

        if (startEnabled)
            Enable();
    }

    public AsmPatch(string sig, byte[] bytes, bool startEnabled = false, bool useASMHook = false)
    {
        var addr = nint.Zero;
        Signature = sig;
        try { addr = DalamudApi.SigScanner.DalamudSigScanner.ScanModule(sig); }
        catch (Exception e) { PluginLog.Warning(e, $"Failed to find signature {sig}"); }
        if (addr == nint.Zero) return;

        Address = addr;
        NewBytes = bytes;
        SafeMemory.ReadBytes(addr, bytes.Length, out var oldBytes);
        OldBytes = oldBytes;
        asmPatches.Add(this);

        if (useASMHook)
            hook = new(addr, NewBytes, $"{Util.AssemblyName.Name} AsmPatch#{asmPatches.Count}", AsmHookBehaviour.DoNotExecuteOriginal);

        if (startEnabled)
            Enable();
    }

    public AsmPatch(string sig, string[] asm, bool startEnabled = false)
    {
        var addr = nint.Zero;
        Signature = sig;
        try { addr = DalamudApi.SigScanner.DalamudSigScanner.ScanModule(sig); }
        catch (Exception e) { PluginLog.Warning(e, $"Failed to find signature {sig}"); }
        if (addr == nint.Zero) return;

        Address = addr;
        SafeMemory.ReadBytes(addr, 7, out var oldBytes);
        OldBytes = oldBytes;
        asmPatches.Add(this);
        hook = new(addr, asm, $"{Util.AssemblyName.Name} AsmPatch#{asmPatches.Count}", AsmHookBehaviour.DoNotExecuteOriginal);

        if (startEnabled)
            Enable();
    }

    public void Enable()
    {
        if (IsEnabled || !IsValid) return;

        if (hook == null)
            SafeMemory.WriteBytes(Address, NewBytes);
        else
            hook.Enable();

        IsEnabled = true;
    }

    public void Disable()
    {
        if (!IsEnabled || !IsValid) return;

        if (hook == null)
            SafeMemory.WriteBytes(Address, OldBytes);
        else
            hook.Disable();

        IsEnabled = false;
    }

    public void Toggle()
    {
        if (!IsEnabled)
            Enable();
        else
            Disable();
    }

    public void Dispose()
    {
        if (IsEnabled)
            Disable();

        if (hook == null) return;
        hook.Dispose();
        SafeMemory.WriteBytes(Address, OldBytes);
    }

    public static void DisposeAll()
    {
        foreach (var patch in asmPatches)
            patch?.Dispose();
    }
}