using System.Runtime.InteropServices;

namespace Hypostasis.Game.Structures;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct CameraManager
{
    [FieldOffset(0x0)] public FFXIVClientStructs.FFXIV.Client.Game.Control.CameraManager CS;
    [FieldOffset(0x0)] public GameCamera* WorldCamera;
    [FieldOffset(0x8)] public GameCamera* IdleCamera;
    [FieldOffset(0x10)] public GameCamera* MenuCamera;
    [FieldOffset(0x18)] public GameCamera* SpectatorCamera;
}
