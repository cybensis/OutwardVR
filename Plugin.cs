using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using Valve.VR;

namespace OutwardVR;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

        InitSteamVR();
        // POSES
        //SteamVR_Actions._default.RightHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, UpdateRightHand);
        //SteamVR_Actions._default.LeftHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, UpdateLeftHand);
        //SteamVR_Actions._default.switchpov.AddOnStateDownListener(OnSwitchPOVDown, SteamVR_Input_Sources.Any);
    }


    // BOOLEANS
    //    public void OnSwitchPOVDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    //{
    //    Logger.LogInfo("wdwdwWDWDWD");
    //    if (AssetLoader.LeftHandBase == null) {
    //        new AssetLoader();
    //    }
    //    CameraManager.SwitchPOV();
    //    CameraManager.SpawnHands();
    //}
    //
    //public void UpdateRightHand(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
    //{
    //    if (CameraManager.RightHand)
    //    {
    //        CameraManager.RightHand.transform.localPosition = fromAction.localPosition;
    //        CameraManager.RightHand.transform.localRotation = fromAction.localRotation;

    //    }

    //}

    //public static void UpdateLeftHand(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
    //{
    //    if (CameraManager.LeftHand)
    //    {
    //        CameraManager.LeftHand.transform.localPosition = fromAction.localPosition;
    //        CameraManager.LeftHand.transform.localRotation = fromAction.localRotation;
    //    }
    //}

    private static void InitSteamVR()
    {
        SteamVR_Actions.PreInitialize();
        SteamVR.Initialize();
        SteamVR_Settings.instance.pauseGameWhenDashboardVisible = true;
    }
}
