using System;
using System.Runtime.InteropServices;

namespace Hypostasis.Game.Structures;

[StructLayout(LayoutKind.Explicit), GameStructure("40 53 48 83 EC 20 48 8D 05 ?? ?? ?? ?? 48 8B D9 48 89 01 48 83 C1 10 E8 ?? ?? ?? ?? 0F B6 83 08 01 00 00 33 C9 24 FD")]
public unsafe partial struct GameCamera : IHypostasisStructure
{
    [FieldOffset(0x0)] public nint* vtbl;
    [FieldOffset(0x60)] public float x;
    [FieldOffset(0x64)] public float y;
    [FieldOffset(0x68)] public float z;
    [FieldOffset(0x90)] public float lookAtX; // Position that the camera is focused on (Actual position when zoom is 0)
    [FieldOffset(0x94)] public float lookAtY;
    [FieldOffset(0x98)] public float lookAtZ;
    [FieldOffset(0x114)] public float currentZoom; // 6
    [FieldOffset(0x118)] public float minZoom; // 1.5
    [FieldOffset(0x11C)] public float maxZoom; // 20
    [FieldOffset(0x120)] public float currentFoV; // 0.78
    [FieldOffset(0x124)] public float minFoV; // 0.69
    [FieldOffset(0x128)] public float maxFoV; // 0.78
    [FieldOffset(0x12C)] public float addedFoV; // 0
    [FieldOffset(0x130)] public float currentHRotation; // -pi -> pi, default is pi
    [FieldOffset(0x134)] public float currentVRotation; // -0.349066
    [FieldOffset(0x138)] public float hRotationDelta;
    [FieldOffset(0x148)] public float minVRotation; // -1.483530, should be -+pi/2 for straight down/up but camera breaks so use -+1.569
    [FieldOffset(0x14C)] public float maxVRotation; // 0.785398 (pi/4)
    [FieldOffset(0x160)] public float tilt;
    [FieldOffset(0x170)] public int mode; // Camera mode? (0 = 1st person, 1 = 3rd person, 2+ = weird controller mode? cant look up/down)
    [FieldOffset(0x174)] public int controlType; // 0 first person, 1 legacy, 2 standard, 3/5/6 ???, 4 ???
    [FieldOffset(0x17C)] public float interpolatedZoom;
    [FieldOffset(0x1B0)] public float viewX;
    [FieldOffset(0x1B4)] public float viewY;
    [FieldOffset(0x1B8)] public float viewZ;
    [FieldOffset(0x1E4)] public byte isFlipped; // 1 while holding the keybind
    [FieldOffset(0x224)] public float lookAtHeightOffset; // No idea what to call this (0x230 is the interpolated value)
    [FieldOffset(0x228)] public byte resetLookatHeightOffset; // No idea what to call this
    [FieldOffset(0x230)] public float interpolatedLookAtHeightOffset;
    [FieldOffset(0x2B4)] public float lookAtY2;

    public bool IsHRotationOffset => mode == isFlipped;
    public float GameObjectHRotation => !IsHRotationOffset ? (currentHRotation > 0 ? currentHRotation - MathF.PI : currentHRotation + MathF.PI) : currentHRotation;
}

/*
public unsafe class VirtualTable
{
    public delegate*<nint, byte, void> vf0; // Dispose
    public delegate*<nint, void> vf1; // Init
    public delegate*<nint, void> vf2; // ??? (new in endwalker)
    public delegate*<nint, void> vf3; // Update
    public delegate*<nint, nint> vf4; // ??? crashes (calls scene camera vf1)
    public delegate*<nint, void> vf5; // reset camera angle
    public delegate*<nint, nint> vf6; // ??? gets something (might need a float array)
    public delegate*<nint, nint> vf7; // ??? get position / rotation? (might need a float array)
    public delegate*<nint, void> vf8; // duplicate of 4
    public delegate*<nint, void> vf9; // ??? (runs whenever the camera is swapped to)
    public delegate*<void> vf10; // empty function (for the world camera anyway) (runs whenever the camera is swapped from)
    public delegate*<void> vf11; // empty function
    public delegate*<nint, nint, bool> vf12; // ??? looks like it returns a bool? (runs whenever the camera gets too close to the character) (compares vf16 return to 2nd argument)
    public delegate*<nint, byte> vf13; // ??? looks like it does something with inputs (returns 0/1 depending on some input)
    public delegate*<void, nint, nint, nint, nint> vf14; // applies center height offset
    public delegate*<nint, nint, nint, byte, void> vf15; // set position (requires 4 arguments, might need a float array)
    public delegate*<nint, byte> vf16; // get control type? returns 1 for legacy, 2 for standard (this value is applied to 0x174)
    public delegate*<nint, nint> vf17; // get camera target
    public delegate*<nint, nint, float> vf18; // ??? crashes
    public delegate*<nint, nint, void> vf19; // ??? requires 2 arguments (might need a float array)
    public delegate*<nint, nint> vf20; // ??? looks like it does something with targeting
    public delegate*<nint, byte, int> vf21; // ??? requires 2 arguments
    public delegate*<nint, bool> vf22; // can change perspective (1st <-> 3rd) (SOMETHING CHANGED IN ENDWALKER)
    public delegate*<nint, void> vf23; // ??? causes a "camera position set" toast with no obvious effect (switch statement with vf15 return)
    public delegate*<nint, void> vf24; // loads the camera angle from 22 (switch statement with vf15 return)
    public delegate*<nint, void> vf25; // causes a "camera position restored to default" toast and causes an effect similar to 1, but doesnt change horizontal angle to default (switch statement with vf15 return)
    public delegate*<nint, float, float> vf26; // ??? places the camera really high above character
    public delegate*<float> vf27; // get max distance? doesnt seem to return anything except 20 ever though
    public delegate*<float> vf28; // get scroll amount (0.75)
    public delegate*<float> vf29; // get ??? (1)
    public delegate*<float> vf30; // get ??? (0.5 or 1) (uses actionmanager/g_layoutworld and uimodule vf87?)
    public delegate*<float> vf31; // duplicate of 28
    public delegate*<float> vf32; // duplicate of 28

    public VirtualTable(nint* address)
    {
        foreach (var f in GetType().GetFields())
        {
            var i = ushort.Parse(f.Name[2..]);
            var vfunc = *(address + i);
            f.SetValue(this, f.FieldType.Cast(vfunc));
        }
    }
}*/