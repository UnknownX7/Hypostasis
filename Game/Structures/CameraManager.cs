using System.Runtime.InteropServices;

namespace Hypostasis.Game.Structures;

[StructLayout(LayoutKind.Explicit), GameStructure("48 89 5C 24 08 57 48 83 EC 20 33 FF 48 8D 05 ?? ?? ?? ?? 48 89 79 28 48 8B D9")]
public unsafe partial struct CameraManager : IHypostasisStructure
{
    [FieldOffset(0x0)] public FFXIVClientStructs.FFXIV.Client.Game.Control.CameraManager CS;
    [FieldOffset(0x0)] public GameCamera* worldCamera;
    [FieldOffset(0x8)] public GameCamera* idleCamera;
    [FieldOffset(0x10)] public GameCamera* menuCamera;
    [FieldOffset(0x18)] public GameCamera* spectatorCamera;

    public bool Validate() => true;
}