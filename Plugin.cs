using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Valve.VR;

namespace OutwardVR;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        PlayerPrefs.SetInt("XBOX_EN", 1);
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        InitSteamVR();
        
        // POSES
        SteamVR_Actions._default.NorthDPAD.AddOnStateDownListener(test, SteamVR_Input_Sources.Any);
        SteamVR_Actions._default.Back.AddOnStateDownListener(t3est, SteamVR_Input_Sources.Any);
        //SteamVR_Actions._default.RightHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, UpdateRightHand);
        //SteamVR_Actions._default.LeftHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, UpdateLeftHand);
        //SteamVR_Actions._default.switchpov.AddOnStateDownListener(OnSwitchPOVDown, SteamVR_Input_Sources.Any);
    }
    public void test(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource) {
        Logs.WriteInfo("DPAD UP");
        Logs.WriteInfo(fromAction.state);

    }

    public void t3est(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Logs.WriteInfo("BACK");
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
