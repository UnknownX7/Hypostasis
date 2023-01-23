using System.Runtime.InteropServices;

namespace Hypostasis.Game.Structures;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Bool
{
    [MarshalAs(UnmanagedType.U1)]
    private readonly bool b;
    public Bool(bool b2) => b = b2;
    public static implicit operator bool(Bool b) => b.b;
    public static implicit operator Bool(bool b) => new(b);
    public override string ToString() => b ? "True" : "False";
}