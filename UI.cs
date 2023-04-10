using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Valve.VR;
using UnityEngine.UI;
using System.Reflection;
using Rewired;
using Rewired.Data;
using Rewired.Data.Mapping;
using NodeCanvas.Framework;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Steamworks;


// 1. MenuManager -> CharacterUIs -> PlayerUI -> Canvas open canvas component and set its render thingy to world space, and set position to cam pos
// 2. In Canvas -> GeneralPanels -> MainScreem -> VisualMainScreen -> Options Set pos to camera
// 3. Set canvas scale to 0.01 for xyz then move it forward on Z +10
// 4. On main camera, set cullingMask to -1 so it shows the HUD

// 1. MenuManager -> CharacterUIs -> PlayerChar -> Canvas open canvas component and set its render thingy to world space, and set position to cam pos
// 2. Set Canvas scale to 0.0005 for xyz then rotate pm 7 180
// 2. When moving you NEED to use canvas because the HUD only exists within the bounds of Canvas so if you move it out of where the canvas exists it disappears


// Move Canvas forward x 0.7 and to the right 0.1 I think??
// Camera.main.transform.position + Camera.main.transform.forward * 0.4f + Camera.main.transform.right * -0.03f;

// From main cam, go up three parents to HeadWhiteMaleA (this will change in game but you should still only need to go up 3 parents) and disable SkinnedMeshRenderer to remove head





namespace OutwardVR
{
    [HarmonyPatch]
    internal class UI
    {
        private const string ASSETBUNDLE_PATH = @"BepInEx\plugins\InwardVR\shaderbundle";
        private static Material AlwaysOnTopMaterial;

        private static Canvas uiWorldCanvas;
        private static RawImage uiRawImage;
        private static readonly RenderTexture uiRenderTexture = new RenderTexture(1920, 1080, 0);
        private static GameObject statusBars;
        private static GameObject quickSlots;
        private static GameObject tempCamHolder = new GameObject();


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterCreationPanel), "Show")]
        public static void PositionCharacterCreationPanel(CharacterCreationPanel __instance) {
            //__instance.CharacterUI;

            Logs.WriteWarning("CreationPanel Show");
            if (tempCamHolder == null)
                tempCamHolder = new GameObject();    
            Camera.main.transform.parent = tempCamHolder.transform;
            tempCamHolder.transform.position = new Vector3(-5000.829f, -5000.1f, -4998.098f);
            tempCamHolder.transform.rotation = new Quaternion(0f, 0.8131f, 0f, -0.5821f);
            __instance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            __instance.transform.root.position = new Vector3(-4997.025f, -5001.101f, -5003.604f);
            __instance.transform.root.rotation = new Quaternion(0, 0.9397f, 0, -0.342f);
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterVisuals), "EquipVisuals")]
        public static void DisableHelmet(CharacterVisuals __instance)
        {
            if (__instance.ActiveVisualsHelmOrHead != null)
                __instance.ActiveVisualsHelmOrHead.Renderer.enabled = false;
        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(ArmorVisuals), "Awake")]
        //public static void DisableHelmetd(ArmorVisuals __instance)
        //{
        //    CharacterVisuals visuals = __instance.transform.parent.GetComponent<CharacterVisuals>();
        //    visuals.ActiveVisualsHelmOrHead.Renderer.enabled = false;
        //}

        //======== UI FIXES ======== //

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(CameraQuality), "Awake")]
        //private static void SetMainMenuPlacementt(CameraQuality __instance) {
        //    __instance.gameObject.active = false;
        //}


        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainScreen), "FirstUpdate")]
        private static void SetMainMenuPlacement(MainScreen __instance)
        {
            //Camera[] cams = Camera.allCameras;
            //for (int i = 0; i < cams.Length; i++) {
            //    cams[i].gameObject.SetActive(true);
            //}
            Logs.WriteWarning("MainScreen FirstUpdate");
            Controllers.Init();
            Canvas menuCanvas = __instance.CharacterUI.transform.parent.GetComponent<Canvas>();
            menuCanvas.renderMode = RenderMode.WorldSpace;
            menuCanvas.transform.root.position = new Vector3(-9.7117f, -3.2f, 4.8f);
            menuCanvas.transform.root.rotation = Quaternion.identity;
            menuCanvas.transform.root.localScale = new Vector3(0.005f, 0.005f, 0.005f);

            //menuCanvas.transform.root.GetChild(2).GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            if (tempCamHolder == null)
                tempCamHolder = new GameObject();
            Camera.main.transform.parent = tempCamHolder.transform;
            Camera.main.cullingMask = -1;
            Camera.main.nearClipPlane = 0.01f;
            tempCamHolder.transform.position = new Vector3(-3.0527f, -2.6422f, 0.1139f);
            tempCamHolder.transform.rotation = new Quaternion(0, 0.342f, 0, 0.9397f);
            Camera.main.transform.rotation = Quaternion.identity;
            __instance.CharacterUI.GetType().GetField("m_currentSelectedGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(__instance.CharacterUI, __instance.FirstSelectable);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainScreen), "Update")]
        private static void UpdateControllersOnMainMenu(MainScreen __instance) {
            Controllers.Update();
        }

            // On MainScreen.Update, update controller inputs

            [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuManager), "Update")]
        private static void CharacterCameraUpdate(MenuManager __instance)
        {

            // I find these values work nicely for positioning the HUD
            LocalCharacterControl characterController = Camera.main.transform.root.GetComponent<LocalCharacterControl>();
            if (characterController != null ) {
                if (characterController.Character.Sneaking)
                    __instance.transform.position = characterController.transform.position + (characterController.transform.right * 0.05f) + (characterController.transform.forward * 0.7f) + (characterController.transform.up * 1.2f);
                else
                    __instance.transform.position = characterController.transform.position + (characterController.transform.forward * 0.6f) + (characterController.transform.up * 1.675f);
                if (characterController.Character.Sprinting)
                    __instance.transform.position += (characterController.transform.forward * 0.2f);

                //__instance.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 0.5f) + (Camera.main.transform.right * -0.05f) + (Camera.main.transform.up * 0.05f);
                __instance.transform.rotation = characterController.transform.rotation;
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterBarListener), "Awake")]
        public static void PositionCharacterBar(CharacterBarListener __instance)
        {
            if (__instance.gameObject.name == "MainCharacterBars") {
                __instance.RectTransform.localPosition = new Vector3(281f, -400, 0f);
                statusBars = __instance.gameObject;

            }

        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(TargetingFlare), "AwakeInit")]
        public static void DisableTargetingFlare(TargetingFlare __instance)
        {
            __instance.gameObject.active = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterBarDisplayHolder), "FreeDisplay")]
        public static void PositionEnemyHealth(CharacterBarDisplayHolder __instance)
        {
            __instance.RectTransform.localPosition = new Vector3(-650f, -1000, 0f);

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ControlsInput), "IsLastActionGamepad")]
        public static bool SetUsingGamepad(ref bool __result) {
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(QuickSlotPanelSwitcher), "StartInit")]
        public static void PositionQuickSlots(QuickSlotPanelSwitcher __instance)
        {
            Vector3 newPos = __instance.transform.localPosition;
            newPos.x = -355f;
            __instance.transform.localPosition = newPos;
            if (__instance.transform.parent.gameObject.name == "QuickSlot") {
                quickSlots = __instance.transform.parent.gameObject;
                quickSlots.SetActive(false);
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterUI), "Update")]
        public static void DisplayQuickSlots(CharacterUI __instance)
        {
            // Display QuickSlots and hide player status bars only if left or right trigger is being held down
            if (SteamVR_Actions._default.LeftTrigger.GetAxis(SteamVR_Input_Sources.Any) > 0.3f || SteamVR_Actions._default.RightTrigger.GetAxis(SteamVR_Input_Sources.Any) > 0.3f)
            {
                if (quickSlots != null)
                    quickSlots.gameObject.SetActive(true);
                if (statusBars != null)
                    statusBars.SetActive(false);
            }
            else {
                if (quickSlots != null)
                    quickSlots.gameObject.SetActive(false);
                if (statusBars != null)
                    statusBars.SetActive(true);
            }

        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterUI), "Awake")]
        public static void SetUIInstance(CharacterUI __instance)
        {
            characterUIInstance = __instance;
        }



        // This only needs to be a onetime thing, find someway to change it so its not on update
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UICompass), "Update")]
        public static void PositionCompass(UICompass __instance)
        {
            Vector3 newPos = __instance.transform.localPosition;
            newPos.y = -450f;
            __instance.transform.localPosition = newPos;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StatusEffectPanel), "AwakeInit")]
        public static void PositionStatusEffectPanel(StatusEffectPanel __instance)
        {
            __instance.transform.localPosition = new Vector3(-250f, 100f, 0f);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NeedsDisplay), "AwakeInit")]
        public static void PositionNeeds(NeedsDisplay __instance)
        {
            __instance.transform.parent.parent.localPosition = new Vector3(411f,100f,0f);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NotificationDisplay), "AwakeInit")]
        public static void PositionNotifications(NotificationDisplay __instance)
        {
            Vector3 newPos = __instance.transform.localPosition;
            newPos.x = -293f;
            __instance.transform.localPosition = newPos;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(TemperatureExposureDisplay), "StartInit")]
        public static void PositionTempDisplay(TemperatureExposureDisplay __instance)
        {
            __instance.transform.localPosition = new Vector3(-208f, -490f, 0f);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(QuiverDisplay), "AwakeInit")]
        public static void PositionQuiverDisplay(QuiverDisplay __instance)
        {
            __instance.transform.localPosition = new Vector3(100f, -525f, 0f);
        }



        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemDisplayDropGround), "Init")]
        public static void PositionMenus(ItemDisplayDropGround __instance)
        {
            __instance.transform.parent.localPosition = new Vector3(-150f, -350f, 0f);
            __instance.transform.parent.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MapDisplay), "AwakeInit")]
        public static void PositionGeneralMenus(MapDisplay __instance)
        {
            Transform GeneralMenus = __instance.transform.parent;
            GeneralMenus.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            GeneralMenus.transform.localRotation = Quaternion.identity;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Tutorialization_UseBandage), "StartInit")]
        public static void PositionBandage(Tutorialization_UseBandage __instance)
        {
            __instance.transform.localPosition = new Vector3(1050f, -160f, 0f);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(UnityEngine.UI.Selectable), "IsHighlighted")]
        public static void SetCurrentButton(UnityEngine.UI.Selectable __instance)
        {
            if (__instance.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
                button = __instance.gameObject.GetComponent<UnityEngine.UI.Button>();

            if (__instance.gameObject.GetComponent<ItemDisplayClick>() != null)
                invItem = __instance.gameObject.GetComponent<ItemDisplayClick>();
            else
                invItem = null;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterUI), "get_EventSystemCurrentSelectedGo")]
        public static void FixContextMenu(CharacterUI __instance, ref GameObject __result)
        {
            // Everytime the context menu (Menu opened when pressing X on an inv item) is opened, it automatically focuses the gamepade controls on a button that is hidden and prevents navigating the menu
            // and this is intended to fix that
            if (__result != null && __result.name == "UI_ContextMenuButton") {
                // Loop over all the context menu items until you find the first child thats active and doesn't have the name Background, as this should be an actual usuable button
                for (int i = 0; i < __result.transform.parent.childCount; i++) {
                    if (__result.transform.parent.GetChild(i).name != "Background" && __result.transform.parent.GetChild(i).gameObject.GetActive()) {
                        GameObject contextButton = __result.transform.parent.GetChild(i).gameObject;
                        // Set the CharacterUI current selected game object to our new button
                        __instance.GetType().GetField("m_currentSelectedGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(__instance, contextButton);
                        // Swap out the result for our new button
                        __result = contextButton;
                        // Just in case the above doesn't work, run Select() on the button
                        contextButton.GetComponent<Button>().Select();
                        // Kill the loop
                        i = __result.transform.parent.childCount;
                    }
                }
            }
            //if (__result.name == "UI_ContextMenuButton") {
            //    Logs.WriteWarning("AAAAAAAAAAA");
            //    GameObject contextButton = __result.transform.parent.GetChild(2).gameObject;
            //    __instance.GetType().GetField("m_currentSelectedGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(__instance, contextButton);
            //    __result = contextButton;
            //}

        }


        

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PointerEventData), "get_pressEventCamera")]
        public static bool SetCamOnPressevent(PointerEventData __instance, ref Camera __result)
        {
            __result = Camera.main;
            return false;
        }

        public static CharacterUI characterUIInstance;
        public static UnityEngine.UI.Button button;
        public static ItemDisplayClick invItem;

        ////// Fix for GroupItemDisplays

        //[HarmonyPatch(typeof(ItemGroupDisplay), "AddItemToGroup")]
        //public class ItemGroupDisplay_AddItemToGroup
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(ItemGroupDisplay __instance)
        //    {
        //        FixUIMaterials(__instance.GetComponentsInChildren<Image>(true),
        //                       __instance.GetComponentsInChildren<Text>(true));
        //    }
        //}

        //private static void FixUIMaterials(Image[] images, Text[] texts)
        //{
        //    foreach (var image in images)
        //    {
        //        if (image.material.name == "Default UI Material")
        //        {
        //            image.material = AlwaysOnTopMaterial;
        //        }
        //    }
        //    foreach (var text in texts)
        //    {
        //        if (text.material.name == "Default UI Material")
        //        {
        //            text.material = AlwaysOnTopMaterial;
        //        }
        //    }
        //}


        ////// Fix MenuManager when character removed

        //[HarmonyPatch(typeof(SplitScreenManager), "RemoveLocalPlayer", new Type[] { typeof(SplitPlayer), typeof(string) })]
        //public class SplitScreenManager_RemoveLocalPlayer
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix(SplitPlayer _player)
        //    {
        //        // todo, check if main player
        //    }
        //}
    }
}
