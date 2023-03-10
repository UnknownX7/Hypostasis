using System;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using Dalamud.Logging;

namespace Hypostasis.Game;

[HypostasisDebuggable]
public sealed class GameFunction<T> : IGameFunction where T : Delegate
{
    public string Signature { get; }
    public nint Address => address ?? SetupAddress(false);
    public T Invoke => Address != nint.Zero ? del ?? SetupDelegate() : null;
    public Hook<T> Hook { get; private set; }
    public T Original => Hook?.Original ?? Invoke;
    public bool IsValid => Invoke != null;
    public bool IsHooked => Hook != null;

    private nint? address;
    private T del;

    public GameFunction(string sig, bool required = false)
    {
        Signature = sig;
        if (required)
            SetupAddress(true);
    }

    private nint SetupAddress(bool required)
    {
        try
        {
            address = DalamudApi.SigScanner.DalamudSigScanner.ScanText(Signature);
        }
        catch (Exception e)
        {
            address = nint.Zero;
            PluginLog.Warning(e, $"Failed to find signature {Signature}");
            if (required)
                throw;
        }

        return address.Value;
    }

    private T SetupDelegate()
    {
        if (address is not > 0) return null;
        return del = Marshal.GetDelegateForFunctionPointer<T>(address.Value);
    }

    public void CreateHook(T detour, bool enable = true, bool dispose = true)
    {
        if (Address == nint.Zero) return;

        if (IsHooked)
            throw new ApplicationException("Attempted to hook function more than once");
        Hook = Hook<T>.FromAddress(Address, detour);
        DalamudApi.SigScanner.AddHook(Hook, enable, dispose);
        //DalamudApi.SigScanner.AddMember(GetType(), this, nameof(Hook));
    }
}