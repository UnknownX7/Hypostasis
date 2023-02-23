using System;
using System.Runtime.InteropServices;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
#pragma warning disable CA2211

namespace Hypostasis.Game.Structures;

[StructLayout(LayoutKind.Explicit), GameStructure("48 89 7C 24 08 45 33 C0 B8 00 00 00 E0")]
public unsafe partial struct ActionManager : IHypostasisStructure
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

    [Signature("E8 ?? ?? ?? ?? 44 8B 4B 2C", Fallibility = Fallibility.Infallible)]
    public static delegate* unmanaged<uint, uint, uint> fpGetSpellIDForAction;
    public static uint GetSpellIDForAction(uint actionType, uint actionID)
    {
        if (fpGetSpellIDForAction == null)
            throw new InvalidOperationException($"InitializeStructure was not called on {nameof(ActionManager)}");
        return fpGetSpellIDForAction(actionType, actionID);
    }

    [Signature("48 89 5C 24 08 57 48 83 EC 20 48 8B DA 8B F9 E8 ?? ?? ?? ?? 4C 8B C3", Fallibility = Fallibility.Infallible)]
    public static delegate* unmanaged<uint, GameObject*, Bool> fpCanUseActionOnGameObject;
    public static bool CanUseActionOnGameObject(uint actionID, GameObject* o)
    {
        if (fpCanUseActionOnGameObject == null)
            throw new InvalidOperationException($"InitializeStructure was not called on {nameof(ActionManager)}");
        return fpCanUseActionOnGameObject(actionID, o) || DalamudApi.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow(actionID) is { TargetArea: true };
    }

    [Signature("E8 ?? ?? ?? ?? 84 C0 74 37 8B 84 24 ?? ?? 00 00", Fallibility = Fallibility.Infallible)]
    public static delegate* unmanaged<ActionManager*, uint, uint, Bool> fpCanQueueAction;
    public bool CanQueueAction(uint actionType, uint actionID)
    {
        if (fpCanQueueAction == null)
            throw new InvalidOperationException($"InitializeStructure was not called on {nameof(ActionManager)}");
        fixed (ActionManager* ptr = &this)
            return fpCanQueueAction(ptr, actionType, actionID);
    }
}