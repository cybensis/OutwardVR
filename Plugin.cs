using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Management;
using Valve.VR;

namespace OutwardVR;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{

    public static string gameExePath = Process.GetCurrentProcess().MainModule.FileName;
    public static string gamePath = Path.GetDirectoryName(gameExePath);
    public static string HMDModel = "";

    public static UnityEngine.XR.Management.XRManagerSettings managerSettings = null;

    public static List<UnityEngine.XR.XRDisplaySubsystemDescriptor> displaysDescs = new List<UnityEngine.XR.XRDisplaySubsystemDescriptor>();
    public static List<UnityEngine.XR.XRDisplaySubsystem> displays = new List<UnityEngine.XR.XRDisplaySubsystem>();
    public static UnityEngine.XR.XRDisplaySubsystem MyDisplay = null;

    public static GameObject SecondEye = null;
    public static Camera SecondCam = null;



    private void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        PlayerPrefs.SetInt("XBOX_EN", 1);


        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

        SteamVR_Actions.PreInitialize();

        SteamVR_Settings.instance.pauseGameWhenDashboardVisible = true;

        var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
        var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
        var xrLoader = ScriptableObject.CreateInstance<OpenVRLoader>();


        var settings = OpenVRSettings.GetSettings();
        settings.StereoRenderingMode = OpenVRSettings.StereoRenderingModes.MultiPass;
        generalSettings.Manager = managerSettings;

        managerSettings.loaders.Clear();
        managerSettings.loaders.Add(xrLoader);
        managerSettings.InitializeLoaderSync(); ;

        XRGeneralSettings.AttemptInitializeXRSDKOnLoad();
        XRGeneralSettings.AttemptStartXRSDKOnBeforeSplashScreen();

        SteamVR.Initialize();


        //If the instance not exit the first time we call the static class


        // POSES
        SteamVR_Actions._default.RightHandPose.AddOnUpdateListener(SteamVR_Input_Sources.RightHand, UpdateRightHand);
        SteamVR_Actions._default.LeftHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, UpdateLeftHand);


        //SteamVR_Actions._default.ClickRightJoystick.AddOnStateDownListener(StartToggleThirdPerson, SteamVR_Input_Sources.Any);
        //SteamVR_Actions._default.ClickRightJoystick.AddOnStateUpListener(EndToggleThirdPerson, SteamVR_Input_Sources.Any);

        //SteamVR_Actions._default.ButtonY.AddOnStateDownListener(InteractDown, SteamVR_Input_Sources.Any);
        //SteamVR_Actions._default.ButtonY.AddOnStateUpListener(InteractUp, SteamVR_Input_Sources.Any);
    }


    //public static void InteractDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    //{
    //    if (InteractionDisplayPatches.laser != null && InteractionDisplayPatches.laser.worldItem != null) { 
    //        Logs.WriteWarning("TEST");
    //        //Camera.main.transform.root.GetComponent<Character>().OnInteractButtonDown();
    //        InteractionDisplayPatches.laser.worldItem.transform.parent.GetComponent<InteractionTriggerBase>().TryActivateBasicAction(Camera.main.transform.root.GetComponent<Character>());
    //    }
    //}
    //public static void InteractUp(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    //{
    //    if (InteractionDisplayPatches.laser != null && InteractionDisplayPatches.laser.worldItem != null)
    //        Camera.main.transform.root.GetComponent<Character>().OnInteractButtonUp();
    //}
    //private static float timeHeldFor = 0;

    //private static void StartToggleThirdPerson(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    //{
    //    timeHeldFor = Time.time;
    //}

    //private static void EndToggleThirdPerson(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    //{
    //    if (Time.time - timeHeldFor > 1)
    //        VRInstanceManager.ToggleThirdPerson();

    //}
    private static void UpdateRightHand(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
    {
        if (CameraManager.RightHand)
        {
            CameraManager.RightHand.transform.localPosition = fromAction.localPosition;
            CameraManager.RightHand.transform.localRotation = fromAction.localRotation;

        }

    }

    private static void UpdateLeftHand(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
    {
        if (CameraManager.LeftHand)
        {
            CameraManager.LeftHand.transform.localPosition = fromAction.localPosition;
            CameraManager.LeftHand.transform.localRotation = fromAction.localRotation;
        }
    }



    //private static void TriggerButton(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    //{
    //    if ((MiscPatches.characterUIInstance.IsMenuFocused || MiscPatches.characterUIInstance.IsDialogueInProgress) && MiscPatches.characterUIInstance.EventSystemCurrentSelectedGo != null)
    //    {


    //        if (MiscPatches.characterUIInstance.EventSystemCurrentSelectedGo.GetComponent<ItemDisplayClick>() != null)
    //            MiscPatches.characterUIInstance.EventSystemCurrentSelectedGo.GetComponent<ItemDisplayClick>().SingleClick();
    //        else if (MiscPatches.characterUIInstance.EventSystemCurrentSelectedGo.GetComponent<UnityEngine.UI.Toggle>() != null)
    //            MiscPatches.characterUIInstance.EventSystemCurrentSelectedGo.GetComponent<UnityEngine.UI.Toggle>().OnSubmit(null);
    //        else if (MiscPatches.characterUIInstance.EventSystemCurrentSelectedGo.GetComponent<Dropdown>() != null)
    //            MiscPatches.characterUIInstance.EventSystemCurrentSelectedGo.GetComponent<Dropdown>().Show();
    //        else if (MiscPatches.characterUIInstance.EventSystemCurrentSelectedGo.GetComponent<Button>() != null)
    //            MiscPatches.characterUIInstance.EventSystemCurrentSelectedGo.GetComponent<Button>().Press();
    //    }
    //}




    //private static void InventoryMenuTrigger(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    //{
    //    if (MiscPatches.characterUIInstance != null &&
    //        MiscPatches.characterUIInstance.IsMenuFocused &&
    //        MiscPatches.characterUIInstance.EventSystemCurrentSelectedGo != null &&
    //        MiscPatches.characterUIInstance.EventSystemCurrentSelectedGo.GetComponent<ItemDisplayClick>() != null)
    //    {
    //        ItemDisplayClick invItem = MiscPatches.characterUIInstance.EventSystemCurrentSelectedGo.GetComponent<ItemDisplayClick>();
    //        PointerEventData _data = new PointerEventData(EventSystem.current);
    //        _data.pointerPress = invItem.gameObject;
    //        // Figure out how to set this value based on the items positon in the inventory canvas
    //        _data.position = new Vector2(1019f, 1143f);
    //        invItem.RightClick(_data);

    //    }
    //}



    private static void InitSteamVR()
    {
        SteamVR_Actions.PreInitialize();
        SteamVR.Initialize();
        SteamVR_Settings.instance.pauseGameWhenDashboardVisible = true;
        SteamVR_Settings.instance.trackingSpace = ETrackingUniverseOrigin.TrackingUniverseSeated;
        SteamVR_Settings.instance.pauseGameWhenDashboardVisible = false;
        SteamVR_Settings.instance.lockPhysicsUpdateRateToRenderFrequency = false;
    }
}
