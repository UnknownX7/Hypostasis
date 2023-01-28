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
                InjectMember("ffxivReplay");
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
                InjectMember("cameraManager");
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
                InjectMember("actionManager");
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
                InjectMember("framework");
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
            AddMember("uiModule");
            return uiModule;
        }
    }

    private static RaptureShellModule* raptureShellModule;
    public static RaptureShellModule* RaptureShellModule
    {
        get
        {
            if (raptureShellModule != null) return raptureShellModule;
            raptureShellModule = UIModule->GetRaptureShellModule();
            AddMember("raptureShellModule");
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
            AddMember("pronounModule");
            return pronounModule;
        }
    }

    [Signature("E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 0F 85 ?? ?? ?? ?? 8D 4F DD")]
    private static delegate* unmanaged<PronounModule*, uint, GameObject*> getGameObjectFromPronounID;
    public static GameObject* GetGameObjectFromPronounID(uint id)
    {
        if (getGameObjectFromPronounID == null)
            InjectMember("getGameObjectFromPronounID");
        return getGameObjectFromPronounID(PronounModule, id);
    }

    public static bool IsMacroRunning => RaptureShellModule->MacroCurrentLine >= 0;

    public static GameObject* UITarget => (GameObject*)*(nint*)((nint)PronounModule + 0x290);

    private static void InjectMember(string member) => DalamudApi.SigScanner.InjectMember(typeof(Common), null, member);

    private static void AddMember(string member) => DalamudApi.SigScanner.AddMember(typeof(Common), null, member);

    public static void Initialize()
    {

    }

    public static void Dispose()
    {

    }
}