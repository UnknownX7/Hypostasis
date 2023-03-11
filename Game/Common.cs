using System.Collections.Generic;
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
    [HypostasisSignatureInjection("48 8D 0D ?? ?? ?? ?? 88 44 24 24", Static = true, Required = true)]
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

    [HypostasisClientStructsInjection<FFXIVClientStructs.FFXIV.Client.Game.Control.CameraManager>(Required = true)]
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

    [HypostasisClientStructsInjection<FFXIVClientStructs.FFXIV.Client.Game.ActionManager>(Required = true)]
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

    [HypostasisClientStructsInjection<Framework>(Required = true)]
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
            return pronounModule;
        }
    }

    public delegate GameObject* GetGameObjectFromPronounIDDelegate(PronounModule* pronounModule, PronounID id);
    public static readonly GameFunction<GetGameObjectFromPronounIDDelegate> getGameObjectFromPronounID = new ("E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 0F 85 ?? ?? ?? ?? 8D 4F DD");
    public static GameObject* GetGameObjectFromPronounID(PronounID id) => getGameObjectFromPronounID.Invoke(PronounModule, id);

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

    public static void Initialize() { }
    public static void Dispose() { }
}