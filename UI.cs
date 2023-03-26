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



        /*  [HarmonyPatch(typeof(MainScreen), "FirstUpdate")]
          public class MainScreen_FirstUpdate
          {
              [HarmonyFinalizer]
              public static Exception Finalizer()
              {
                  SetupCharacterUI();
                  return null;
              }
          }*/

        //======== UI FIXES ======== //

        /*  [HarmonyPostfix]
          [HarmonyPatch(typeof(CharacterCamera), "Update")]
          private static void CharacterCamera_Update(CharacterCamera __instance, Camera ___m_camera)
          {
              // Maybe change this so it tracks the whole MenuManager to the camera??
              Canvas UICanvas = __instance.TargetCharacter.CharacterUI.UIPanel.gameObject.GetComponent<Canvas>();
              if (UICanvas)
              {
                  //UICanvas.transform.root.position = Camera.main.transform.position + (Camera.main.transform.forward * 0.1f) + (Camera.main.transform.right * -0.48f) + (Camera.main.transform.up * -0.1f);
                  UICanvas.transform.root.position = Camera.main.transform.position + (Camera.main.transform.forward * 0.1f) + (Camera.main.transform.right * -0.5f) + (Camera.main.transform.up * -0.1f);
                  UICanvas.transform.root.rotation = Camera.main.transform.rotation;

                  //UICanvas.transform.root.position = Camera.main.transform.position + (Camera.main.transform.forward * 0.4f) + (Camera.main.transform.right * -0.035f) + (Camera.main.transform.up * -0.025f);
                  //UICanvas.transform.root.rotation = Camera.main.transform.rotation;
              }
          }
  */

        private static int i = 1;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuManager), "Update")]
        private static void CharacterCamera_Update(MenuManager __instance, RectTransform ___m_characterUIHolder)
        {
            // Maybe change this so it tracks the whole MenuManager to the camera??

            //__instance.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 0.1f) + (Camera.main.transform.right * -0.5f) + (Camera.main.transform.up * -0.1f);
            if (i == 1) {
                //__instance.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
                __instance.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 0.5f) + (Camera.main.transform.right * -0.05f) + (Camera.main.transform.up * 0.05f);
                __instance.transform.rotation = Camera.main.transform.rotation;
                //___m_characterUIHolder.position = __instance.transform.position;
                //i++;
            }


        }



        private static void SetupUIShader()
        {
            // Load bundle
            var bundle = AssetBundle.LoadFromFile(ASSETBUNDLE_PATH);
            AlwaysOnTopMaterial = bundle.LoadAsset<Material>("UI_AlwaysOnTop");

            //// Fix loaded images and text

            //FixUIMaterials(Resources.FindObjectsOfTypeAll<Image>(),
            //               Resources.FindObjectsOfTypeAll<Text>());

            //// Fix UIUtilities prefabs

            //var prefabs = new MonoBehaviour[] { UIUtilities.ItemDisplayPrefab, UIUtilities.ItemDetailPanel };
            //foreach (var obj in prefabs)
            //{
            //    FixUIMaterials(obj.GetComponentsInChildren<Image>(true),
            //                   obj.GetComponentsInChildren<Text>(true));
            //}
        }

        //[HarmonyPatch(typeof(CharacterManager), "OnAllPlayersDoneLoading")]
        //public class CharacterManager_OnAllPlayersDoneLoading
        //{
        //    [HarmonyPostfix]
        //    public static void Postfix()
        //    {
        //        SetupCharacterUI();
        //    }
        //}

/*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterUI), "Awake")]
        public static void FixUI(CharacterUI __instance, RectTransform ___m_gameplayPanelsHolder)
        {

            Canvas UICanvas = __instance.UIPanel.gameObject.GetComponent<Canvas>();
            if (UICanvas)
            {
                UICanvas.renderMode = RenderMode.WorldSpace;
                UICanvas.transform.localScale = new Vector3(0.0003f, 0.0003f, 0.0003f);
                Camera.main.cullingMask = -1;
                Camera.main.nearClipPlane = 0.001f;
            }
        }
*/


        // Need to move this UI moving function somewhere else cos when the player moves or runs, the hud updates slowly so it keeps going into the userwwwww
        /*        [HarmonyPostfix]
                [HarmonyPatch(typeof(CharacterUI), "Update")]
                public static void PositionUI(CharacterUI __instance, RectTransform ___m_gameplayPanelsHolder)
                {
                    // Move Canvas forward x 0.7 and to the right 0.1 I think??
                    // Camera.main.transform.position + Camera.main.transform.forward * 0.4f + Camera.main.transform.right * -0.03f;

                    Canvas UICanvas = __instance.UIPanel.gameObject.GetComponent<Canvas>();
                    if (UICanvas && ___m_gameplayPanelsHolder.gameObject.GetActive())
                    {
                        UICanvas.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 0.4f) + (Camera.main.transform.right * -0.035f)  + (Camera.main.transform.up * -0.025f);
                        UICanvas.transform.rotation = Camera.main.transform.rotation;
                    }

                    // Set HUD MainCharacterBars LocalPosition to 606.6805 300.9597. This thingy uses the CharacterBarListener class so maybe do something with that
                }*/

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterBarListener), "Awake")]
        public static void PositionCharacterBar(CharacterBarListener __instance)
        {
            

            //__instance.RectTransform.localPosition = new Vector3(281f, -150, 0f);
            __instance.RectTransform.localPosition = new Vector3(281f, -400, 0f);
            if (__instance.gameObject.name == "MainCharacterBars") { 
                statusBars = __instance.gameObject;
                Logs.WriteWarning("Character bars found");

            }

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
                Logs.WriteWarning("Quick Slot Found");
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterUI), "Update")]
        public static void DisplayQuickSlots(CharacterUI __instance)
        {
            //Logs.WriteInfo(SteamVR_Actions._default.LeftTrigger.GetAxis(SteamVR_Input_Sources.Any));
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
            Vector3 newPos = __instance.transform.localPosition;
            newPos.y = 250f;
            __instance.transform.localPosition = new Vector3(-365f, 200f, 0f);
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

        // QuickSlotPanelSwitcher
        // x -355

        // UICompass
        // Y -450

        //StatusEffectPanel
        // Y 250


        // NeedsDisplay.RectTransform.parent.parent
        // 411.7643 100 0


        //NotificationDisplay
        // x -293

        // TemperatureExposureDisplay
        // -208.6015 -490.5 0

        //ItemDisplayDropGround.transform.parent
        // -175.0001 -300 0
        // scale = 0.8 0.8 0.8

        // Blackfade.gameObject.transform
        // Z 400

        //private static void SetupCharacterUI()
        //{
        //    Debug.Log("[InwardVR] setting up UI...");

        //    try
        //    {

        //    }
        //    catch (Exception e)
        //    {
        //        Debug.Log("ERROR setting up InwardVR UI...");
        //        Debug.Log(e);
        //    }
        //}


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
