using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Rewired;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Valve.VR;
using static MapMagic.ObjectPool;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.Management;
using Unity.XR.OpenVR;
using UnityEngine.XR;
using UnityEngine.UI;

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


    //Create a class that actually inherits from MonoBehaviour
    public class MyStaticMB : MonoBehaviour
    {
        protected virtual void Update()
        {
            //if (Camera.main)
            //{
            //    CameraPatcher.HandleStereoRendering();
            //}
        }
    }

    ////Variable reference for the class
    public static MyStaticMB myStaticMB;

    private void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        PlayerPrefs.SetInt("XBOX_EN", 1);

        new AssetLoader();

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
        // Use when eventually trying to get motion controls going
        //SteamVR_Actions._default.ClickRightJoystick.AddOnStateDownListener(ww, SteamVR_Input_Sources.Any);
        //SteamVR_Actions._default.RightHandPose.AddOnUpdateListener(SteamVR_Input_Sources.RightHand, UpdateRightHand);
        //SteamVR_Actions._default.LeftHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, UpdateLeftHand);



        SteamVR_Actions._default.ButtonA.AddOnStateDownListener(TriggerButton, SteamVR_Input_Sources.Any);
        SteamVR_Actions._default.ButtonX.AddOnStateDownListener(InventoryMenuTrigger, SteamVR_Input_Sources.Any);
        SteamVR_Actions._default.ButtonB.AddOnStateDownListener(RemoveActiveButton, SteamVR_Input_Sources.Any);
    }


    public static void ww(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        CameraManager.Setup();
    }



    public static void UpdateRightHand(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
    {
        if (CameraManager.RightHand)
        {
            // Maybe to fix the hand position offset stuff, add a check if the hands are below a certain Y axis, if they are then add until they are at the minimum Y, which is like maybe
            // just where your hands would be having your arms at your side.
            //Vector3 newPos = Camera.main.transform.parent.parent.position;
            //newPos.y -= 0.5f;
            //CameraManager.VROrigin.transform.position = newPos;
            CameraManager.RightHand.transform.localPosition = fromAction.localPosition;
            CameraManager.RightHand.transform.localRotation = fromAction.localRotation;

        }

    }

    public static void UpdateLeftHand(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
    {
        if (CameraManager.LeftHand)
        {
            //Vector3 newPos = Camera.main.transform.parent.parent.position;
            //newPos.y -= 0.5f;
            //CameraManager.VROrigin.transform.position = newPos;
            CameraManager.LeftHand.transform.localPosition = fromAction.localPosition;
            CameraManager.LeftHand.transform.localRotation = fromAction.localRotation;
        }
    }



    public static void TriggerButton(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        if (UI.characterUIInstance.IsMenuFocused || UI.characterUIInstance.IsDialogueInProgress)
        {
            // if ui.IsMenuFocused
            // if ui.CurrentSelectedGameObject has UISelectable allow for A input and if it has ItemDisplayClick allow for X
            if (UI.characterUIInstance.IsOptionPanelDisplayed && UI.dropdown != null) {
                UI.dropdown.Show();
                UI.dropdown = null;
            }
            else if (UI.characterUIInstance.IsOptionPanelDisplayed && UI.dropdownItem != null)
            {
                UI.dropdownItem.GetComponent<UnityEngine.UI.Toggle>().OnSubmit(null);
                UI.dropdownItem = null;
                Logs.WriteError("DropdownItem pressed");
            }

            if (UI.button != null)
            {
                UI.button.Press();
                UI.button = null;
                //var reflection = UI.button.GetType().GetMethod("Press", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //reflection.Invoke(UI.button, new object[] { });
            }
            
            if (UI.characterUIInstance.CurrentSelectedGameObject != null && UI.characterUIInstance.CurrentSelectedGameObject.GetComponent<ItemDisplayClick>() != null)
            {
                ItemDisplayClick invItem = UI.characterUIInstance.CurrentSelectedGameObject.GetComponent<ItemDisplayClick>();
                invItem.SingleClick();
                //var reflection = invItem.GetType().GetMethod("SingleClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //reflection.Invoke(invItem, new object[] { });
            }

        }

    }




    public static void InventoryMenuTrigger(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {

        if (UI.characterUIInstance != null &&
            UI.characterUIInstance.IsMenuFocused &&
            UI.characterUIInstance.CurrentSelectedGameObject != null &&
            UI.characterUIInstance.CurrentSelectedGameObject.GetComponent<ItemDisplayClick>() != null)
        {
            ItemDisplayClick invItem = UI.characterUIInstance.CurrentSelectedGameObject.GetComponent<ItemDisplayClick>();
            PointerEventData _data = new PointerEventData(EventSystem.current);
            _data.pointerPress = invItem.gameObject;
            // Figure out how to set this value based on the items positon in the inventory canvas
            _data.position = new Vector2(1019f, 1143f);

            var reflection = invItem.GetType().GetMethod("RightClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            reflection.Invoke(invItem, new object[] { _data });
        }

    }


    public static void RemoveActiveButton(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        UI.button = null;
    }


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
