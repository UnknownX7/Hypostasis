using System.Runtime.InteropServices;

namespace Hypostasis.Game.Structures;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Bool
{
    [MarshalAs(UnmanagedType.U1)]
    private readonly bool b;
    public Bool(byte b2) => b = b2 != 0;
    public Bool(bool b2) => b = b2;
    public static implicit operator bool(Bool b) => b.b;
    public static implicit operator Bool(bool b) => new(b);
    public static implicit operator byte(Bool b) => (byte)(b.b ? 1 : 0);
    public static implicit operator Bool(byte b) => new(b);
    public override string ToString() => b ? "True" : "False";
}