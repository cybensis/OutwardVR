//using HarmonyLib;
//using UnityEngine;
//using Valve.VR;
//using UnityEngine.UI;
//using UnityEngine.Events;
//using UnityEngine.EventSystems;
//using UnityEngine.SceneManagement;


//namespace OutwardVR
//{
//    [HarmonyPatch]
//    internal class UI
//    {
//        private const string ASSETBUNDLE_PATH = @"BepInEx\plugins\InwardVR\shaderbundle";
//        private static Material AlwaysOnTopMaterial;

//        private static Canvas uiWorldCanvas;
//        private static RawImage uiRawImage;
//        private static readonly RenderTexture uiRenderTexture = new RenderTexture(1920, 1080, 0);
//        private static GameObject statusBars;
//        private static GameObject quickSlots;
//        private static GameObject tempCamHolder = new GameObject();
//        public static GameObject loadingCam;
//        public static bool gameHasBeenLoadedOnce = false;

//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(CharacterCreationPanel), "Show")]
//        public static void PositionCharacterCreationPanel(CharacterCreationPanel __instance) {
//            //__instance.CharacterUI;

//            Logs.WriteWarning("CreationPanel Show");
//            if (tempCamHolder == null)
//                tempCamHolder = new GameObject();    
//            Camera.main.transform.parent = tempCamHolder.transform;
//            if (gameHasBeenLoadedOnce)
//                tempCamHolder.transform.position = new Vector3(-5000.329f, -4998.899f, -4997.397f);
//            else
//                tempCamHolder.transform.position = new Vector3(-5000.329f, -4998.599f, -4997.397f);
//            tempCamHolder.transform.rotation = new Quaternion(-0.0201f, 0.8114f, 0.1211f, -0.5715f);
//            __instance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

//            if (gameHasBeenLoadedOnce)
//                __instance.transform.root.position = new Vector3(-5000.929f, -4998.499f, -5000.4f);
//            else
//                __instance.transform.root.position = new Vector3(-4997.025f, -5001.101f, -5003.604f);
//            __instance.transform.root.rotation = new Quaternion(0, 0.9397f, 0, -0.342f);

//            Transform GeneralMenus = __instance.CharacterUI.transform.root.GetChild(2); // Maybe change this to loop over all children, its place might change
//            if (GeneralMenus.name == "GeneralMenus")
//            {
//                GeneralMenus.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
//                //GeneralMenus.transform.localPosition = Vector3.zero;
//                GeneralMenus.transform.position = __instance.CharacterUI.transform.position;
//                GeneralMenus.transform.rotation = Quaternion.identity;
//                GeneralMenus.transform.localScale = new Vector3(1, 1, 1);
//            }
//        }


//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(ProloguePanel), "Show", new[] { typeof(EventContextData.ContextScreen[]), typeof(UnityAction) })]
//        public static void PositionIntroCanvas(ProloguePanel __instance) {
//            Logs.WriteWarning("POSITION INTRO CANVAS");
//            if (gameHasBeenLoadedOnce)
//                tempCamHolder.transform.position = new Vector3(-8f, -3f, -2f);
//            else
//                tempCamHolder.transform.position = new Vector3(-16.5f, -3f, 0);

//            tempCamHolder.transform.rotation = new Quaternion(-0.1158f, 0.3311f, -0.0594f, 0.9346f);

//            if (__instance.CharacterUI != null)
//                __instance.CharacterUI.gameObject.active = false;
//        }

//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(CharacterVisuals), "EquipVisuals")]
//        public static void DisableHelmet(CharacterVisuals __instance)
//        {
//            if (__instance.ActiveVisualsHelmOrHead != null)
//                __instance.ActiveVisualsHelmOrHead.Renderer.enabled = false;
//        }


//        //======== UI FIXES ======== //



//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(MainScreen), "FirstUpdate")]
//        private static void SetMainMenuPlacement(MainScreen __instance)
//        {
//            // When returning from the game to the main menu, it deletes the controller scheme so we have to reset it here
//            Controllers.ResetControllerVars();
//            Controllers.Init();

//            //Get all the game objects from the current scene so we can find the "Main Camera" since it doesn't appear in Camera.allCameras
//            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
//            Camera mainCam = Camera.main;
//            for (int i = 0; i < rootObjects.Length; i++)
//            {
//                if (rootObjects[i].name == "Main Camera(Clone)")
//                    rootObjects[i].gameObject.SetActive(false);
//                else if (rootObjects[i].name == "Main Camera") {
//                    rootObjects[i].gameObject.SetActive(true);
//                    mainCam = rootObjects[i].GetComponent<Camera>();
//                }
//            }
            
//            if (tempCamHolder == null)
//                tempCamHolder = new GameObject();
//            tempCamHolder.transform.position = new Vector3(-3.4527f, -1.3422f, 0.1139f);
//            tempCamHolder.transform.rotation = new Quaternion(-0.1504f, 0.3658f, -0.0555f, 0.9168f);
//            UnityEngine.Object.DontDestroyOnLoad(tempCamHolder);


//            Canvas menuCanvas = __instance.CharacterUI.transform.parent.GetComponent<Canvas>();
//            menuCanvas.renderMode = RenderMode.WorldSpace;
//            if (gameHasBeenLoadedOnce)
//                menuCanvas.transform.root.position = new Vector3(-4.5687f, -0.1414f, 5.1412f);
//            else
//                menuCanvas.transform.root.position = new Vector3(-9.7117f, -3.2f, 4.8f);

//            menuCanvas.transform.root.rotation = Quaternion.identity;
//            menuCanvas.transform.root.localScale = new Vector3(0.005f, 0.005f, 0.005f);

//            mainCam.transform.parent = tempCamHolder.transform;
//            mainCam.cullingMask = -1;
//            mainCam.nearClipPlane = 0.01f;



//            Transform GeneralMenus = menuCanvas.transform.root.GetChild(2); // Maybe change this to loop over all children, its place might change
//            if (GeneralMenus.name == "GeneralMenus")
//            {
//                GeneralMenus.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
//                //GeneralMenus.transform.localPosition = Vector3.zero;
//                GeneralMenus.transform.position = menuCanvas.transform.position;
//                GeneralMenus.transform.rotation = Quaternion.identity;
//                GeneralMenus.transform.localScale = new Vector3(1, 1, 1);
//            }



//            if (loadingCam == null) {
//                loadingCam = new GameObject();
//                loadingCam.transform.parent = tempCamHolder.transform;
//                loadingCam.AddComponent<Camera>();
//            }
//            // Keep loadingCam disabled until loading is triggered
//            loadingCam.active = false;

//            __instance.CharacterUI.GetType().GetField("m_currentSelectedGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(__instance.CharacterUI, __instance.FirstSelectable);
//        }

//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(MainScreen), "Update")]
//        private static void UpdateControllersOnMainMenu(MainScreen __instance) {
//            Controllers.Update();
//        }




//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(MenuManager), "Update")]
//        private static void CharacterCameraUpdate(MenuManager __instance)
//        {

//            // I find these values work nicely for positioning the HUD
//            if (Camera.main == null || Camera.main.transform.root == null)
//                return;
//            Character character = Camera.main.transform.root.GetComponent<Character>();
//            if (character != null ) {
//                if (character.Sneaking)
//                    __instance.transform.position = character.transform.position + (character.transform.right * 0.05f) + (character.transform.forward * 0.7f) + (character.transform.up * 1.2f);
//                else
//                    __instance.transform.position = character.transform.position + (character.transform.forward * 0.6f) + (character.transform.up * 1.675f);
//                if (character.Sprinting)
//                    __instance.transform.position += (character.transform.forward * 0.2f);
//                else if (character.AnimMove.x < 0.1f)
//                    __instance.transform.position += (character.transform.forward * 0.1f);
//                __instance.transform.rotation = character.transform.rotation;
//            }


//        }


//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(MenuManager), "BackToMainMenu")]
//        public static void PositionCamOnReturnToMenu(MenuManager __instance)
//        {
//            Logs.WriteWarning("RETURNING TO MENU");
//            loadingCam.active = true;
//            tempCamHolder.transform.position = new Vector3(-8.4527f, -3.4422f, -0.7861f);
//            Camera mainCam = loadingCam.GetComponent<Camera>();
//            mainCam.cullingMask = 32;
//            mainCam.clearFlags = CameraClearFlags.SolidColor;
//            mainCam.backgroundColor = Color.black;
//            mainCam.nearClipPlane = 0.01f;
//            mainCam.depth = 10;
//            __instance.transform.position = new Vector3(-4.5687f, -0.1414f, 5.1412f);
//        }


//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(MenuManager), "ShowMasterLoadingScreen", new[] { typeof(string) } )] 
//        public static void PositionCamOnLoad(MenuManager __instance)
//        {
//            Logs.WriteWarning("SHOW MASTER LOADING SCREEN");
//            loadingCam.active = true;

//            tempCamHolder.transform.position = new Vector3(-8.4527f, -3.4422f, -0.7861f);
//            Camera mainCam = loadingCam.GetComponent<Camera>();
//            mainCam.cullingMask = 32;
//            mainCam.clearFlags = CameraClearFlags.SolidColor;
//            mainCam.backgroundColor = Color.black;
//            mainCam.nearClipPlane = 0.01f;
//            mainCam.depth = 10;
//            if (gameHasBeenLoadedOnce)
//                __instance.transform.position = new Vector3(-4.5687f, -0.1414f, 5.1412f);
//            else
//                __instance.transform.position = new Vector3(-9.7117f, -3.2f, 4.8f);

//            Logs.WriteWarning(__instance.IsProloguePanelDisplayed);


//        }


//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(CharacterBarListener), "Awake")]
//        public static void PositionCharacterBar(CharacterBarListener __instance)
//        {
//            if (__instance.gameObject.name == "MainCharacterBars") {
//                __instance.RectTransform.localPosition = new Vector3(281f, -400, 0f);
//                statusBars = __instance.gameObject;

//            }

//        }


//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(TargetingFlare), "AwakeInit")]
//        public static void DisableTargetingFlare(TargetingFlare __instance)
//        {

//            __instance.gameObject.active = false;
//        }

//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(CharacterBarDisplayHolder), "FreeDisplay")]
//        public static void PositionEnemyHealth(CharacterBarDisplayHolder __instance)
//        {
//            __instance.RectTransform.localPosition = new Vector3(-650f, -1000, 0f);

//        }

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(ControlsInput), "IsLastActionGamepad")]
//        public static bool SetUsingGamepad(ref bool __result) {
//            __result = true;
//            return false;
//        }

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(QuickSlotPanelSwitcher), "StartInit")]
//        public static void PositionQuickSlots(QuickSlotPanelSwitcher __instance)
//        {
//            Vector3 newPos = __instance.transform.localPosition;
//            newPos.x = -355f;
//            __instance.transform.localPosition = newPos;
//            if (__instance.transform.parent.gameObject.name == "QuickSlot") {
//                quickSlots = __instance.transform.parent.gameObject;
//                quickSlots.SetActive(false);
//            }
//        }


//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(CharacterUI), "Update")]
//        public static void DisplayQuickSlots(CharacterUI __instance)
//        {
//            // Display QuickSlots and hide player status bars only if left or right trigger is being held down
//            if (SteamVR_Actions._default.LeftTrigger.GetAxis(SteamVR_Input_Sources.Any) > 0.3f || SteamVR_Actions._default.RightTrigger.GetAxis(SteamVR_Input_Sources.Any) > 0.3f)
//            {
//                if (quickSlots != null)
//                    quickSlots.gameObject.SetActive(true);
//                if (statusBars != null)
//                    statusBars.SetActive(false);
//            }
//            else {
//                if (quickSlots != null)
//                    quickSlots.gameObject.SetActive(false);
//                if (statusBars != null)
//                    statusBars.SetActive(true);
//            }

//        }



//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(CharacterUI), "Awake")]
//        public static void SetUIInstance(CharacterUI __instance)
//        {
//            characterUIInstance = __instance;
//        }



//        // This only needs to be a onetime thing, find someway to change it so its not on update
//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(UICompass), "Update")]
//        public static void PositionCompass(UICompass __instance)
//        {
//            Vector3 newPos = __instance.transform.localPosition;
//            newPos.y = -450f;
//            __instance.transform.localPosition = newPos;
//        }

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(StatusEffectPanel), "AwakeInit")]
//        public static void PositionStatusEffectPanel(StatusEffectPanel __instance)
//        {
//            __instance.transform.localPosition = new Vector3(-250f, 100f, 0f);
//        }

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(NeedsDisplay), "AwakeInit")]
//        public static void PositionNeeds(NeedsDisplay __instance)
//        {
//            __instance.transform.parent.parent.localPosition = new Vector3(411f,100f,0f);
//        }

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(NotificationDisplay), "AwakeInit")]
//        public static void PositionNotifications(NotificationDisplay __instance)
//        {
//            Vector3 newPos = __instance.transform.localPosition;
//            newPos.x = -293f;
//            __instance.transform.localPosition = newPos;
//        }


//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(TemperatureExposureDisplay), "StartInit")]
//        public static void PositionTempDisplay(TemperatureExposureDisplay __instance)
//        {
//            __instance.transform.localPosition = new Vector3(-208f, -490f, 0f);
//        }


//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(QuiverDisplay), "AwakeInit")]
//        public static void PositionQuiverDisplay(QuiverDisplay __instance)
//        {
//            __instance.transform.localPosition = new Vector3(100f, -525f, 0f);
//        }



//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(ItemDisplayDropGround), "Init")]
//        public static void PositionMenus(ItemDisplayDropGround __instance)
//        {
//            __instance.transform.parent.localPosition = new Vector3(-150f, -350f, 0f);
//            __instance.transform.parent.localScale = new Vector3(0.8f, 0.8f, 0.8f);
//        }


//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(MapDisplay), "AwakeInit")]
//        public static void PositionGeneralMenus(MapDisplay __instance)
//        {
//            Transform GeneralMenus = __instance.transform.parent;
//            GeneralMenus.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
//            GeneralMenus.transform.localRotation = Quaternion.identity;
//        }

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(Tutorialization_UseBandage), "StartInit")]
//        public static void PositionBandage(Tutorialization_UseBandage __instance)
//        {
//            __instance.transform.localPosition = new Vector3(1050f, -160f, 0f);
//        }


//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(UnityEngine.UI.Selectable), "IsHighlighted")]
//        public static void SetCurrentButton(UnityEngine.UI.Selectable __instance)
//        {
//            if (__instance.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
//                button = __instance.gameObject.GetComponent<UnityEngine.UI.Button>();

//            if (__instance.gameObject.GetComponent<ItemDisplayClick>() != null)
//                invItem = __instance.gameObject.GetComponent<ItemDisplayClick>();
//            else
//                invItem = null;
//        }


//        [HarmonyPostfix]
//        [HarmonyPatch(typeof(CharacterUI), "get_EventSystemCurrentSelectedGo")]
//        public static void FixContextMenu(CharacterUI __instance, ref GameObject __result)
//        {
//            // Everytime the context menu (Menu opened when pressing X on an inv item) is opened, it automatically focuses the gamepade controls on a button that is hidden and prevents navigating the menu
//            // and this is intended to fix that
//            if (__result != null && __result.name == "UI_ContextMenuButton") {
//                // Loop over all the context menu items until you find the first child thats active and doesn't have the name Background, as this should be an actual usuable button
//                for (int i = 0; i < __result.transform.parent.childCount; i++) {
//                    if (__result.transform.parent.GetChild(i).name != "Background" && __result.transform.parent.GetChild(i).gameObject.GetActive()) {
//                        GameObject contextButton = __result.transform.parent.GetChild(i).gameObject;
//                        // Set the CharacterUI current selected game object to our new button
//                        __instance.GetType().GetField("m_currentSelectedGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(__instance, contextButton);
//                        // Swap out the result for our new button
//                        __result = contextButton;
//                        // Just in case the above doesn't work, run Select() on the button
//                        contextButton.GetComponent<Button>().Select();
//                        // Kill the loop
//                        i = __result.transform.parent.childCount;
//                    }
//                }
//            }
//            //if (__result.name == "UI_ContextMenuButton") {
//            //    Logs.WriteWarning("AAAAAAAAAAA");
//            //    GameObject contextButton = __result.transform.parent.GetChild(2).gameObject;
//            //    __instance.GetType().GetField("m_currentSelectedGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(__instance, contextButton);
//            //    __result = contextButton;
//            //}

//        }


        

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(PointerEventData), "get_pressEventCamera")]
//        public static bool SetCamOnPressEvent(PointerEventData __instance, ref Camera __result)
//        {
//            __result = Camera.main;
//            return false;
//        }

//        public static CharacterUI characterUIInstance;
//        public static UnityEngine.UI.Button button;
//        public static ItemDisplayClick invItem;

//        ////// Fix for GroupItemDisplays

//        //[HarmonyPatch(typeof(ItemGroupDisplay), "AddItemToGroup")]
//        //public class ItemGroupDisplay_AddItemToGroup
//        //{
//        //    [HarmonyPostfix]
//        //    public static void Postfix(ItemGroupDisplay __instance)
//        //    {
//        //        FixUIMaterials(__instance.GetComponentsInChildren<Image>(true),
//        //                       __instance.GetComponentsInChildren<Text>(true));
//        //    }
//        //}

//        //private static void FixUIMaterials(Image[] images, Text[] texts)
//        //{
//        //    foreach (var image in images)
//        //    {
//        //        if (image.material.name == "Default UI Material")
//        //        {
//        //            image.material = AlwaysOnTopMaterial;
//        //        }
//        //    }
//        //    foreach (var text in texts)
//        //    {
//        //        if (text.material.name == "Default UI Material")
//        //        {
//        //            text.material = AlwaysOnTopMaterial;
//        //        }
//        //    }
//        //}


//        ////// Fix MenuManager when character removed

//        //[HarmonyPatch(typeof(SplitScreenManager), "RemoveLocalPlayer", new Type[] { typeof(SplitPlayer), typeof(string) })]
//        //public class SplitScreenManager_RemoveLocalPlayer
//        //{
//        //    [HarmonyPostfix]
//        //    public static void Postfix(SplitPlayer _player)
//        //    {
//        //        // todo, check if main player
//        //    }
//        //}
//    }
//}
