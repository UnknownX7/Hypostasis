using System;
using System.Collections.Generic;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using Hypostasis.Game.Structures;

#pragma warning disable CS0649

namespace Hypostasis.Game;

public static unsafe class Common
{
    [Signature("48 8D 0D ?? ?? ?? ?? 88 44 24 24", ScanType = ScanType.StaticAddress, Fallibility = Fallibility.Infallible)]
    private static FFXIVReplay* ffxivReplay;
    public static FFXIVReplay* FFXIVReplay
    {
        get
        {
            if (ffxivReplay == null)
                InjectMember(nameof(ffxivReplay));
            return ffxivReplay;
        }
    }

    [ClientStructs<FFXIVClientStructs.FFXIV.Client.Game.Control.CameraManager>]
    private static CameraManager* cameraManager;
    public static CameraManager* CameraManager
    {
        get
        {
            if (cameraManager == null)
                InjectMember(nameof(cameraManager));
            return cameraManager;
        }
    }

    [ClientStructs<FFXIVClientStructs.FFXIV.Client.Game.ActionManager>]
    private static ActionManager* actionManager;
    public static ActionManager* ActionManager
    {
        get
        {
            if (actionManager == null)
                InjectMember(nameof(actionManager));
            return actionManager;
        }
    }

    [ClientStructs<Framework>]
    private static Framework* framework;
    public static Framework* Framework
    {
        get
        {
            if (framework == null)
                InjectMember(nameof(framework));
            return framework;
        }
    }

    private static UIModule* uiModule;
    public static UIModule* UIModule
    {
        get
        {
            if (uiModule != null) return uiModule;
            uiModule = Framework->UIModule;
            AddMember(nameof(uiModule));
            return uiModule;
        }
    }

    private static InputData* inputData;
    public static InputData* InputData
    {
        get
        {
            if (inputData != null) return inputData;
            inputData = (InputData*)UIModule->GetUIInputData();
            AddMember(nameof(inputData));
            return inputData;
        }
    }

    private static RaptureShellModule* raptureShellModule;
    public static RaptureShellModule* RaptureShellModule
    {
        get
        {
            if (raptureShellModule != null) return raptureShellModule;
            raptureShellModule = UIModule->GetRaptureShellModule();
            AddMember(nameof(raptureShellModule));
            return raptureShellModule;
        }
    }

    private static PronounModule* pronounModule;
    public static PronounModule* PronounModule
    {
        get
        {
            if (pronounModule != null) return pronounModule;
            pronounModule = UIModule->GetPronounModule();
            AddMember(nameof(pronounModule));
            return pronounModule;
        }
    }

    [Signature("E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 0F 85 ?? ?? ?? ?? 8D 4F DD")]
    public static delegate* unmanaged<PronounModule*, PronounID, GameObject*> fpGetGameObjectFromPronounID;
    public static GameObject* GetGameObjectFromPronounID(PronounID id)
    {
        if (fpGetGameObjectFromPronounID == null)
            InjectMember(nameof(fpGetGameObjectFromPronounID));
        return fpGetGameObjectFromPronounID(PronounModule, id);
    }

    public static IEnumerable<nint> GetPartyMembers()
    {
        static nint f(uint i) => (nint)GetGameObjectFromPronounID((PronounID)(43 + i));
        for (uint i = 0; i < 8; i++)
        {
            var address = f(i);
            if (address != nint.Zero)
                yield return address;
        }
    }

    public static IEnumerable<nint> GetEnemies()
    {
        static nint f(uint i) => (nint)GetGameObjectFromPronounID((PronounID)(9 + i));
        for (uint i = 0; i < 26; i++)
        {
            var address = f(i);
            if (address != nint.Zero)
                yield return address;
        }
    }

    public static bool IsMacroRunning => RaptureShellModule->MacroCurrentLine >= 0;

    public static GameObject* UITarget => (GameObject*)*(nint*)((nint)PronounModule + 0x290);

    private static void InjectMember(string member) => DalamudApi.SigScanner.InjectMember(typeof(Common), null, member);

    private static void AddMember(string member) => DalamudApi.SigScanner.AddMember(typeof(Common), null, member);

    public static void InitializeStructure<T>(bool infallible = true) where T : IHypostasisStructure
    {
        try
        {
            DalamudApi.SigScanner.Inject(typeof(T), null, false);
        }
        catch (Exception e)
        {
            if (infallible)
                throw;
            PluginLog.Warning(e, "Failed loading structure");
        }
    }

    public static void InitializeStructures(bool infallible, params Type[] types)
    {
        try
        {
            foreach (var type in types)
                DalamudApi.SigScanner.Inject(type, null, false);
        }
        catch (Exception e)
        {
            if (infallible)
                throw;
            PluginLog.Warning(e, "Failed loading structure");
        }
    }

    public static void Initialize()
    {

    }

    public static void Dispose()
    {

    }
}