using System.Runtime.InteropServices;

namespace Hypostasis.Game.Structures;

[StructLayout(LayoutKind.Explicit)]
public struct ActionManager
{
    [FieldOffset(0x0)] public FFXIVClientStructs.FFXIV.Client.Game.ActionManager CS;
    [FieldOffset(0x8)] public float animationLock;
    [FieldOffset(0x28)] public bool isCasting;
    [FieldOffset(0x28)] public uint castActionType;
    [FieldOffset(0x2C)] public uint castActionID;
    [FieldOffset(0x30)] public float elapsedCastTime;
    [FieldOffset(0x34)] public float castTime;
    [FieldOffset(0x38)] public long castTargetObjectID;
    [FieldOffset(0x60)] public float remainingComboTime;
    [FieldOffset(0x68)] public bool isQueued;
    [FieldOffset(0x98)] public long queuedGroundTargetObjectID;
    [FieldOffset(0xB8)] public byte activateGroundTarget;
    [FieldOffset(0x110)] public ushort currentSequence;
    //[FieldOffset(0x112)] public ushort unknownSequence; // ???
    [FieldOffset(0x5E8)] public bool isGCDRecastActive;
    [FieldOffset(0x5EC)] public uint currentGCDAction;
    [FieldOffset(0x5F0)] public float elapsedGCDRecastTime;
    [FieldOffset(0x5F4)] public float gcdRecastTime;
}