namespace Hypostasis.Game;

public abstract unsafe class VirtualTable
{
    protected readonly nint* vtbl;
    protected VirtualTable(nint* v) => vtbl = v;
    public nint this[int i] => vtbl[i];
}