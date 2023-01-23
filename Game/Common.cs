using Dalamud.Utility.Signatures;
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

    [ClientStructs(typeof(FFXIVClientStructs.FFXIV.Client.Game.Control.CameraManager), "Instance")]
    private static CameraManager* cameraManager; // g_ControlSystem_CameraManager
    public static CameraManager* CameraManager
    {
        get
        {
            if (cameraManager == null)
                InjectMember("cameraManager");
            return cameraManager;
        }
    }

    [ClientStructs(typeof(FFXIVClientStructs.FFXIV.Client.Game.ActionManager), "Instance")]
    private static ActionManager* actionManager;
    public static ActionManager* ActionManager
    {
        get
        {
            if (actionManager == null)
                InjectMember("Instance");
            return actionManager;
        }
    }


    private static void InjectMember(string member) => DalamudApi.SigScanner.InjectMember(typeof(Common), null, member);

    public static void Initialize()
    {
        DalamudApi.SigScanner.Inject(typeof(Common));
    }

    public static void Dispose()
    {

    }
}