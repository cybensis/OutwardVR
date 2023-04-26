using HarmonyLib;
using UnityEngine;
using Valve.VR;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static AQUAS_Parameters;
using ParadoxNotion.Services;
using static MapMagic.ObjectPool;
using NodeCanvas.Framework;


namespace OutwardVR
{
    [HarmonyPatch]
    internal class UI
    {
        private static GameObject statusBars;
        private static GameObject quickSlots;
        private static GameObject tempCamHolder = new GameObject("tempCamHolder");
        private static GameObject enemyHealthHolder;
        private static GameObject newCharacterCamHolder = new GameObject("newCharacterCamHolder");
        public static GameObject loadingCamHolder;
        private static GameObject menuManager;
        public static bool gameHasBeenLoadedOnce = false;
        public static bool isLoading = false;



        [HarmonyPostfix]
        [HarmonyPatch(typeof(UICompass), "Update")]
        private static void FixCompassDirection(UICompass __instance)
        {
            if (Camera.main != null && __instance != null)
                __instance.TargetTransform = Camera.main.transform;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterBarListener), "UpdateDisplay")]
        private static void HelpInitEnemyHealthBar(CharacterBarListener __instance)
        {
            // Since the health bar object no longer spawns in the MenuManager object it needs some
            // help initialising its values.
            if (__instance.gameObject.name == "CharacterBar(Clone)" && __instance.m_characterUI == null)
                    __instance.m_characterUI = characterUIInstance;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterBarListener), "UpdateDisplay")]
        private static void PositionEnemyHealth(CharacterBarListener __instance)
        {
            // This check for the name is because the player health bar also uses CharacterBarListener
            if (__instance.gameObject.name == "CharacterBar(Clone)")
            {
                if (__instance.transform.parent != null && __instance.transform.parent.parent.name != "enemyHealthHolder") {
                    __instance.transform.parent.SetParent(enemyHealthHolder.transform);
                    __instance.transform.parent.localScale = new Vector3(0.005f, 0.005f, 0.005f);
                    __instance.transform.parent.localRotation = Quaternion.identity;
                    __instance.transform.localRotation = Quaternion.identity;
                }
                __instance.transform.parent.localPosition = Vector3.zero;
                __instance.transform.localPosition = Vector3.zero;
                // Get the enemies position
                Vector3 barPosition = __instance.TargetCharacter.transform.position;
                // Get their center height, then multiply it by two and add it to Y so it always appears exactly above their head
                barPosition.y += __instance.TargetCharacter.CenterHeight * 2;
                enemyHealthHolder.transform.position = barPosition;
                enemyHealthHolder.transform.rotation = Camera.main.transform.root.rotation;
            }
        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(CharacterCamera), "LateUpdate")]
        //private static void TrackCamToPhysicalHead(CharacterCreationPanel __instance)
        //{

        //    if (!NetworkLevelLoader.Instance.IsOverallLoadingDone)
        //        return;

        //    if (FirstPersonCamera.playerHead != null)
        //    {
        //        Vector3 camPosition = FirstPersonCamera.playerHead.transform.position;
        //        camPosition = (camPosition - Camera.main.transform.parent.parent.position);
        //        camPosition.x = Mathf.Round(camPosition.x * 100) / 100;
        //        camPosition.y = 1;
        //        camPosition.z = Mathf.Round(camPosition.z * 100) / 100;
        //        Camera.main.transform.parent.parent.localPosition = camPosition;
        //    }
        //}
        public static void PositionMenuAfterLoading() {
            if (menuManager.transform.parent != null)
            {
                Logs.WriteWarning("POSITION MENU AFTER LOADING");
                tempCamHolder.transform.position = menuManager.transform.root.position + (menuManager.transform.root.right * -0.15f) + (menuManager.transform.root.root.up * 0.3f) + (menuManager.transform.root.forward * -1.5f);
                menuManager.transform.root.localRotation = Quaternion.identity;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterCreationPanel), "PutBackCamera")]
        private static void ReturnCameraFromCharacterCreation(CharacterCreationPanel __instance) {
            Camera.main.transform.parent = tempCamHolder.transform;
            PositionMenuManager(__instance.transform.root.gameObject);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterCreationPanel), "Show")]
        private static void PositionCharacterCreationPanel(CharacterCreationPanel __instance)
        {
            if (newCharacterCamHolder == null)
                newCharacterCamHolder = new GameObject("newCharacterCamHolder");
            Camera.main.transform.parent = newCharacterCamHolder.transform;

            newCharacterCamHolder.transform.position = new Vector3(-4998.8f, -4999.5f, -4997.397f);
            newCharacterCamHolder.transform.rotation = Quaternion.identity;
            newCharacterCamHolder.transform.Rotate(0, 200, 0);
            __instance.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            __instance.transform.root.position = new Vector3(-5000.929f, -4998.499f, -5000.4f);


            __instance.transform.root.rotation = new Quaternion(0, 0.9397f, 0, -0.342f);
            Transform GeneralMenus = __instance.CharacterUI.transform.root.GetChild(2); // Maybe change this to loop over all children, its place might change
            if (GeneralMenus.name == "GeneralMenus")
            {
                GeneralMenus.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                //GeneralMenus.transform.localPosition = Vector3.zero;
                GeneralMenus.transform.position = __instance.CharacterUI.transform.position;
                GeneralMenus.transform.rotation = Quaternion.identity;
                GeneralMenus.transform.localScale = new Vector3(1, 1, 1);
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(ProloguePanel), "Show", new[] { typeof(EventContextData.ContextScreen[]), typeof(UnityAction) })]
        private static void PositionIntroCanvas(ProloguePanel __instance)
        {
            Logs.WriteWarning("POSITION INTRO CANVAS");
            if (gameHasBeenLoadedOnce)
                tempCamHolder.transform.position = new Vector3(-8f, -3f, -2f);
            else
                tempCamHolder.transform.position = new Vector3(-16.5f, -3f, 0);

            tempCamHolder.transform.rotation = new Quaternion(-0.1158f, 0.3311f, -0.0594f, 0.9346f);

            if (__instance.CharacterUI != null)
                __instance.CharacterUI.gameObject.active = false;

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterVisuals), "EquipVisuals")]
        private static void DisableHelmet(CharacterVisuals __instance)
        {
            try
            {
                if (__instance.ActiveVisualsHelmOrHead != null && __instance.ActiveVisualsHelmOrHead.Renderer != null)
                    __instance.ActiveVisualsHelmOrHead.Renderer.enabled = false;
                if (__instance.DefaultHairVisuals != null && __instance.DefaultHairVisuals.gameObject != null)
                    __instance.DefaultHairVisuals.gameObject.active = false;
            }
            catch {
                return;
            }
        }


        private static void PositionMenuManager(GameObject menuManager) {

            Logs.WriteWarning("Position menu");

            tempCamHolder.transform.rotation = Quaternion.identity;
            menuManager.transform.rotation = Quaternion.identity;
            switch (chosenTitleScreen)
            {
                case CAVE_LOADING_SCREEN:
                    tempCamHolder.transform.position = new Vector3(-2.7527f, -2.7422f, -1.4861f);
                    tempCamHolder.transform.rotation = new Quaternion(0f, 0.1736f, 0f, -0.9848f);
                    menuManager.transform.position = new Vector3(-7.4117f, 0f, 5.7f);
                    menuManager.transform.rotation = new Quaternion(0f, 0.1736f, 0f, -0.9848f);
                    break;
                case TABLE_LOADING_SCREEN:
                    tempCamHolder.transform.position = new Vector3(-3.8527f, -2.2422f, -1.2861f);
                    menuManager.transform.position = new Vector3(-6.4117f, 0.5f, 6.7f);
                    menuManager.transform.rotation = new Quaternion(0, -0.0872f, 0, 0.9962f);

                    break;
                case VOLCANO_LOADING_SCREEN:
                    tempCamHolder.transform.position = new Vector3(-4.2527f, -1.8422f, -2.3861f);
                    menuManager.transform.position = new Vector3(-4.9117f, 0f, 4.8f);
                    break;
                case CLIFFSIDE_LOADING_SCREEN:
                    tempCamHolder.transform.position = new Vector3(-3.1527f, -2.2422f, -2.2861f);
                    menuManager.transform.position = new Vector3(-0.9527f, 0.2578f, 6.5139f);
                    menuManager.transform.rotation = new Quaternion(0, 0.2164f, 0, 0.9763f);
                    break;
                case HUNTING_LOADING_SCREEN:
                    tempCamHolder.transform.position = new Vector3(-2.4527f, -2.0422f, -0.5861f);
                    tempCamHolder.transform.rotation = new Quaternion(0, 0.1736f, 0, -0.9848f);
                    menuManager.transform.position = new Vector3(-8.8117f, 0.1f, 6.7f);
                    menuManager.transform.rotation = new Quaternion(0f, 0.2588f, 0f, -0.9659f);
                    break;
                case TOWNSQUARE_LOADING_SCREEN:
                    tempCamHolder.transform.position = new Vector3(-2.4527f, -2.6422f, -2.3861f);
                    menuManager.transform.localScale = new Vector3(0.003f, 0.003f, 0.003f);
                    menuManager.transform.position = new Vector3(-5.3117f, 0f, 2.1f);
                    menuManager.transform.Rotate(0, 341, 2);
                    break;
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainScreen), "FirstUpdate")]
        private static void SetMainMenuPlacement(MainScreen __instance)
        {
            Logs.WriteWarning("Main menu first update");


            if (enemyHealthHolder == null)
            {
                enemyHealthHolder = new GameObject("enemyHealthHolder");
                enemyHealthHolder.AddComponent<Canvas>();
                UnityEngine.Object.DontDestroyOnLoad(enemyHealthHolder);
            }

            // When returning from the game to the main menu, it deletes the controller scheme so we have to reset it here
            Controllers.ResetControllerVars();
            Controllers.Init();

            Camera mainCam = Camera.main;

            if (tempCamHolder == null)
                tempCamHolder = new GameObject("tempCamHolder");
            UnityEngine.Object.DontDestroyOnLoad(tempCamHolder);


            Canvas menuCanvas = __instance.CharacterUI.transform.parent.GetComponent<Canvas>();
            menuCanvas.renderMode = RenderMode.WorldSpace;
            
            // These 3 lines ensure that no matter what size the users browser is, the worldspace UI stays the same resolution, that way its position never changes
            // when the user changes their resolution
            menuCanvas.GetComponent<RectTransform>().localScale = new Vector3(1,1,1);
            menuCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
            menuCanvas.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);

            menuCanvas.transform.root.localScale = new Vector3(0.005f, 0.005f, 0.005f);

            mainCam.transform.parent = tempCamHolder.transform;
            mainCam.cullingMask = -1;
            mainCam.nearClipPlane = FirstPersonCamera.NEAR_CLIP_PLANE_VALUE;
            mainCam.targetTexture = null;
            mainCam.gameObject.AddComponent<SteamVR_TrackedObject>();
            tempCamHolder.transform.Rotate(0, -25, 0);


            Transform GeneralMenus = menuCanvas.transform.root.GetChild(2); // Maybe change this to loop over all children, its place might change
            if (GeneralMenus.name == "GeneralMenus")
            {
                GeneralMenus.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                //GeneralMenus.transform.localPosition = Vector3.zero;
                GeneralMenus.transform.position = menuCanvas.transform.position;
                GeneralMenus.transform.localRotation = Quaternion.identity;
                GeneralMenus.transform.localScale = new Vector3(1, 1, 1);
            }

            if (loadingCamHolder == null)
            {
                loadingCamHolder = new GameObject("loadingCam");
                loadingCamHolder.transform.parent = tempCamHolder.transform;
                loadingCamHolder.AddComponent<Camera>();
                loadingCamHolder.AddComponent<SteamVR_TrackedObject>();
            }
            // Keep loadingCamHolder disabled until loading is triggered
            loadingCamHolder.active = false;

            //__instance.CharacterUI.GetType().GetField("m_currentSelectedGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(__instance.CharacterUI, __instance.FirstSelectable);
            menuManager = __instance.transform.root.gameObject;

            PositionMenuManager(menuManager);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainScreen), "Update")]
        private static void UpdateControllersOnMainMenu(MainScreen __instance)
        {
            Controllers.Update();
        }




        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuManager), "Update")]
        private static void PositionHUD(MenuManager __instance)
        {

            try { 
                if (Camera.main == null || Camera.main.transform.root == null || Camera.main.transform.root.GetComponent<Character>() == null)
                    return;
                Character character = Camera.main.transform.root.GetComponent<Character>();
                // By setting the HUD's parent to the head object it rotates with the body and by setting the local position, it is positioned perfectly
                if (__instance.transform.parent == null) {
                    __instance.transform.parent = Camera.main.transform.parent.parent.parent.transform;
                    __instance.transform.localRotation = Quaternion.identity;
                }
                __instance.transform.localPosition = new Vector3(-0.025f, 1.6f, 0.6f);
                if (character.Sneaking)
                    __instance.transform.localPosition += new Vector3(0, -0.4f, 0);
            }
            catch {
                return;
            }

        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuManager), "BackToMainMenu")]
        private static void PositionCamOnReturnToMenu(MenuManager __instance)
        {
            Logs.WriteWarning("RETURNING TO MENU");
            __instance.transform.parent.DetachChildren();
            loadingCamHolder.active = true;
            tempCamHolder.transform.position = new Vector3(-3.5f, -1.25f, -0.7861f);
            tempCamHolder.transform.rotation = Quaternion.identity;
            Camera loadingCam  = loadingCamHolder.GetComponent<Camera>();
            loadingCam.cullingMask = 32;
            loadingCam.clearFlags = CameraClearFlags.SolidColor;
            loadingCam.backgroundColor = Color.black;
            loadingCam.nearClipPlane = FirstPersonCamera.NEAR_CLIP_PLANE_VALUE;
            loadingCam.depth = 10;
            __instance.transform.position = tempCamHolder.transform.position + (tempCamHolder.transform.right * -0.5f) + (tempCamHolder.transform.up * 1.25f) + (tempCamHolder.transform.forward * 5f);
            __instance.transform.localRotation = Quaternion.identity;
            __instance.transform.rotation = Quaternion.identity;

        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuManager), "ShowMasterLoadingScreen", new[] { typeof(string) })]
        private static void PositionCamOnLoad(MenuManager __instance)
        {
            Logs.WriteWarning("SHOW MASTER LOADING SCREEN");
            loadingCamHolder.active = true;
            if (__instance.transform.parent != null)
            {
                PositionMenuAfterLoading();
            }
            else
            {
                tempCamHolder.transform.position = new Vector3(-3.5f, -1.25f, -0.7861f);
                tempCamHolder.transform.rotation = Quaternion.identity;
                __instance.transform.localRotation = Quaternion.identity;
                __instance.transform.position = tempCamHolder.transform.position + (tempCamHolder.transform.right * -0.5f) + (tempCamHolder.transform.up * 1.25f) + (tempCamHolder.transform.forward * 5f);
                Camera loadingCam = loadingCamHolder.GetComponent<Camera>();
                loadingCam.cullingMask = 32;
                loadingCam.clearFlags = CameraClearFlags.SolidColor;
                loadingCam.backgroundColor = Color.black;
                loadingCam.nearClipPlane = FirstPersonCamera.NEAR_CLIP_PLANE_VALUE;
                loadingCam.depth = 10;
                Transform GeneralMenus = __instance.transform.GetChild(2); // Maybe change this to loop over all children, its place might change
                if (GeneralMenus.name == "GeneralMenus")
                {
                    GeneralMenus.rotation = Quaternion.identity;
                    GeneralMenus.localRotation = Quaternion.identity;
                }
            }

            isLoading = true;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterBarListener), "Awake")]
        private static void PositionCharacterBar(CharacterBarListener __instance)
        {
            if (__instance.gameObject.name == "MainCharacterBars")
            {
                __instance.RectTransform.localPosition = new Vector3(281f, -400, 0f);
                statusBars = __instance.gameObject;
            }

        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(TargetingFlare), "AwakeInit")]
        private static void DisableTargetingFlare(TargetingFlare __instance)
        {
            __instance.gameObject.active = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterBarDisplayHolder), "FreeDisplay")]
        private static void PositionEnemyHealth(CharacterBarDisplayHolder __instance)
        {
            __instance.RectTransform.localPosition = new Vector3(-650f, -1000, 0f);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ControlsInput), "IsLastActionGamepad")]
        private static bool SetUsingGamepad(ref bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(QuickSlotPanelSwitcher), "StartInit")]
        private static void PositionQuickSlots(QuickSlotPanelSwitcher __instance)
        {
            Vector3 newPos = __instance.transform.localPosition;
            newPos.x = -355f;
            __instance.transform.localPosition = newPos;
            if (__instance.transform.parent.gameObject.name == "QuickSlot")
            {
                quickSlots = __instance.transform.parent.gameObject;
                quickSlots.SetActive(false);
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterUI), "Update")]
        private static void DisplayQuickSlots(CharacterUI __instance)
        {
            // Display QuickSlots and hide player status bars only if left or right trigger is being held down
            if (SteamVR_Actions._default.LeftTrigger.GetAxis(SteamVR_Input_Sources.Any) > 0.3f || SteamVR_Actions._default.RightTrigger.GetAxis(SteamVR_Input_Sources.Any) > 0.3f)
            {
                if (quickSlots != null)
                    quickSlots.gameObject.SetActive(true);
                if (statusBars != null)
                    statusBars.SetActive(false);
            }
            else
            {
                if (quickSlots != null)
                    quickSlots.gameObject.SetActive(false);
                if (statusBars != null)
                    statusBars.SetActive(true);
            }

        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterUI), "Awake")]
        private static void SetUIInstance(CharacterUI __instance)
        {
            characterUIInstance = __instance;
        }



        //This only needs to be a onetime thing, find someway to change it so its not on update
       [HarmonyPrefix]
       [HarmonyPatch(typeof(UICompass), "Update")]
        private static void PositionCompass(UICompass __instance)
        {
            Vector3 newPos = __instance.transform.localPosition;
            newPos.y = -450f;
            __instance.transform.localPosition = newPos;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(LowStaminaListener), "Awake")]
        private static void HideLowStaminaEffect(LowStaminaListener __instance)
        {
            __instance.gameObject.active = false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(StatusEffectPanel), "AwakeInit")]
        private static void PositionStatusEffectPanel(StatusEffectPanel __instance)
        {
            __instance.transform.localPosition = new Vector3(-250f, 100f, 0f);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NeedsDisplay), "AwakeInit")]
        private static void PositionNeeds(NeedsDisplay __instance)
        {
            __instance.transform.parent.parent.localPosition = new Vector3(411f, 100f, 0f);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NotificationDisplay), "AwakeInit")]
        private static void PositionNotifications(NotificationDisplay __instance)
        {
            Vector3 newPos = __instance.transform.localPosition;
            newPos.x = -293f;
            __instance.transform.localPosition = newPos;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(TemperatureExposureDisplay), "StartInit")]
        private static void PositionTempDisplay(TemperatureExposureDisplay __instance)
        {
            __instance.transform.localPosition = new Vector3(-208f, -490f, 0f);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(QuiverDisplay), "AwakeInit")]
        private static void PositionQuiverDisplay(QuiverDisplay __instance)
        {
            __instance.transform.localPosition = new Vector3(100f, -525f, 0f);
        }



        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemDisplayDropGround), "Init")]
        private static void PositionMenus(ItemDisplayDropGround __instance)
        {
            __instance.transform.parent.localPosition = new Vector3(-150f, -350f, 0f);
            __instance.transform.parent.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MapDisplay), "AwakeInit")]
        private static void PositionGeneralMenus(MapDisplay __instance)
        {
            Transform GeneralMenus = __instance.transform.parent;
            GeneralMenus.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            GeneralMenus.transform.localRotation = Quaternion.identity;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Tutorialization_UseBandage), "StartInit")]
        private static void PositionBandage(Tutorialization_UseBandage __instance)
        {
            __instance.transform.localPosition = new Vector3(1050f, -160f, 0f);
        }


        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(Selectable), "IsHighlighted")]
        //public static void SetCurrentButton(Selectable __instance)
        //{
        //    Logs.WriteWarning("Is Highlighted");
        //    Logs.WriteWarning(__instance.gameObject.GetComponent<UnityEngine.UI.Button>().name);
        //    if (__instance.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
        //        button = __instance.gameObject.GetComponent<UnityEngine.UI.Button>();

        //    if (__instance.gameObject.GetComponent<ItemDisplayClick>() != null)
        //        invItem = __instance.gameObject.GetComponent<ItemDisplayClick>();
        //    else
        //        invItem = null;
        //}

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UnityEngine.UI.Selectable), "OnSelect")]
        private static void SetCurrentButton(UnityEngine.UI.Selectable __instance)
        {
            if (__instance.gameObject.GetComponent<UnityEngine.UI.Button>() != null) 
                button = __instance.gameObject.GetComponent<UnityEngine.UI.Button>();
            else
                button = null;
            
            if (__instance.gameObject.GetComponent<UnityEngine.UI.Dropdown>() != null)
                dropdown = __instance.gameObject.GetComponent<UnityEngine.UI.Dropdown>();
            else    
                dropdown = null;

            if (__instance.gameObject.GetComponent<Dropdown.DropdownItem>() != null)
                dropdownItem = __instance.gameObject.GetComponent<Dropdown.DropdownItem>();
            else
                dropdownItem = null;

            if (__instance.gameObject.GetComponent<ItemDisplayClick>() != null)
                invItem = __instance.gameObject.GetComponent<ItemDisplayClick>();
            else
                invItem = null;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(DialoguePanel), "SkipLine")]
        private static bool PreventAccidentalDialogueSkip(DialoguePanel __instance)
        {

            // When selecting an option from the dialogue panel, it usually automatically skips the following piece of dialogue
            // so set a time check here to prevent it unless a second has passed already
            if (UnityEngine.Time.time - __instance.m_timeOfLastSelectedChoice > 1)
                __instance.m_activeDialogue[0].Continue();
            return false;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterUI), "get_EventSystemCurrentSelectedGo")]
        private static void FixContextMenu(CharacterUI __instance, ref GameObject __result)
        {
            // Everytime the context menu (Menu opened when pressing X on an inv item) is opened, it automatically focuses the gamepade controls on a button that is hidden and prevents navigating the menu
            // and this is intended to fix that
            if (__result != null && __result.name == "UI_ContextMenuButton")
            {
                // Loop over all the context menu items until you find the first child thats active and doesn't have the name Background, as this should be an actual usuable button
                for (int i = 0; i < __result.transform.parent.childCount; i++)
                {
                    if (__result.transform.parent.GetChild(i).name != "Background" && __result.transform.parent.GetChild(i).gameObject.GetActive())
                    {
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
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TitleScreenLoader), "LoadTitleScreen")]
        private static void PositionDifferentMenus(TitleScreenLoader __instance, object[] __args) {
            chosenTitleScreen = __args[0] as string;
            Logs.WriteWarning("LOADING TITLE SCREEN");
            // When returning from a game to the main menu, the main menu FirstUpdate gets ran before
            if (gameHasBeenLoadedOnce) { 
                
                PositionMenuManager(menuManager);
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(PointerEventData), "get_pressEventCamera")]
        // Pretty sure this is used for activating the inventory context menu because we need to create a fake pointer event when activating the
        // context menu but can't set the camera manually, so we need to do that here.
        private static bool SetCamOnPressEvent(PointerEventData __instance, ref Camera __result)
        {
            __result = Camera.main;
            return false;
        }

        private static string chosenTitleScreen;
        public static CharacterUI characterUIInstance;
        public static UnityEngine.UI.Button button;
        public static Dropdown dropdown;
        public static Dropdown.DropdownItem dropdownItem;
        public static ItemDisplayClick invItem;

        private const string CAVE_LOADING_SCREEN = "0";
        private const string TABLE_LOADING_SCREEN = "A";
        private const string VOLCANO_LOADING_SCREEN = "B";
        private const string CLIFFSIDE_LOADING_SCREEN = "C";
        private const string HUNTING_LOADING_SCREEN = "D";
        private const string TOWNSQUARE_LOADING_SCREEN = "E";
    }
}
