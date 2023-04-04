using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Rewired;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;
using static MapMagic.ObjectPool;

namespace OutwardVR;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        // Plugin startup logic
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        PlayerPrefs.SetInt("XBOX_EN", 1);

        new AssetLoader();

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        InitSteamVR();

        // POSES

        //SteamVR_Actions._default.ClickRightJoystick.AddOnStateDownListener(ww, SteamVR_Input_Sources.Any);



        SteamVR_Actions._default.ButtonA.AddOnStateDownListener(TriggerButton, SteamVR_Input_Sources.Any);
        SteamVR_Actions._default.ButtonX.AddOnStateDownListener(InventoryMenuTrigger, SteamVR_Input_Sources.Any);
        SteamVR_Actions._default.ButtonB.AddOnStateDownListener(RemoveActiveButton, SteamVR_Input_Sources.Any);

        SteamVR_Actions._default.RightHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, UpdateRightHand);
        SteamVR_Actions._default.LeftHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, UpdateLeftHand);
    }

    public static void ww(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        CameraManager.Setup();
    }



    public static void UpdateRightHand(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
    {
        if (CameraManager.RightHand)
        {
            Vector3 newPos = Camera.main.transform.parent.parent.position;
            newPos.y -= 0.5f;
            CameraManager.VROrigin.transform.position = newPos;
            CameraManager.RightHand.transform.localPosition = fromAction.localPosition;
            CameraManager.RightHand.transform.localRotation = fromAction.localRotation;

        }

    }

    public static void UpdateLeftHand(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
    {
        if (CameraManager.LeftHand)
        {
            Vector3 newPos = Camera.main.transform.parent.parent.position;
            newPos.y -= 0.5f;
            CameraManager.VROrigin.transform.position = newPos;
            CameraManager.LeftHand.transform.localPosition = fromAction.localPosition;
            CameraManager.LeftHand.transform.localRotation = fromAction.localRotation;
        }
    }



    public static void TriggerButton(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {

        if (UI.characterUIInstance.IsMenuFocused) { 
            // if ui.IsMenuFocused
                // if ui.CurrentSelectedGameObject has UISelectable allow for A input and if it has ItemDisplayClick allow for X
            if (UI.button != null)
            {
                var reflection = UI.button.GetType().GetMethod("Press", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                reflection.Invoke(UI.button, new object[] { });
            }
            if (UI.characterUIInstance.CurrentSelectedGameObject != null && UI.characterUIInstance.CurrentSelectedGameObject.GetComponent<ItemDisplayClick>() != null) 
            {
                ItemDisplayClick invItem = UI.characterUIInstance.CurrentSelectedGameObject.GetComponent<ItemDisplayClick>();
                var reflection = invItem.GetType().GetMethod("SingleClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                reflection.Invoke(invItem, new object[] { });
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
            _data.position = new Vector2(1019f, 1143f);

            var reflection = invItem.GetType().GetMethod("RightClick", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            reflection.Invoke(invItem, new object[] { _data });
        }

    }


    public static void RemoveActiveButton(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        UI.button = null;
    }

    public static void LogBinds()
    {
        Controllers.LogAllGameActions(ReInput.players.AllPlayers[1]);
    }

    private static void InitSteamVR()
    {
        SteamVR_Actions.PreInitialize();
        SteamVR.Initialize();
        SteamVR_Settings.instance.pauseGameWhenDashboardVisible = true;
    }
}


