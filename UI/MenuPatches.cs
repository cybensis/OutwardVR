using HarmonyLib;
using UnityEngine.Events;
using UnityEngine;
using Valve.VR;
using OutwardVR.camera;
using UnityEngine.UI;

namespace OutwardVR.UI
{
    [HarmonyPatch]
    internal class MenuPatches
    {
        private static GameObject tempCamHolder = new GameObject("tempCamHolder");
        private static GameObject enemyHealthHolder;
        private static GameObject newCharacterCamHolder = new GameObject("newCharacterCamHolder");
        public static GameObject loadingCamHolder;
        private static GameObject menuManager;

        private const string CAVE_LOADING_SCREEN = "0";
        private const string TABLE_LOADING_SCREEN = "A";
        private const string VOLCANO_LOADING_SCREEN = "B";
        private const string CLIFFSIDE_LOADING_SCREEN = "C";
        private const string HUNTING_LOADING_SCREEN = "D";
        private const string TOWNSQUARE_LOADING_SCREEN = "E";
        private static string chosenTitleScreen;


        public static void PositionMenuAfterLoading()
        {
            if (MenuManager.Instance.transform.parent != null)
            {
                MenuManager.Instance.transform.root.localRotation = Quaternion.identity;
                tempCamHolder.transform.rotation = Quaternion.identity;
                MenuManager.Instance.m_masterLoading.transform.parent.rotation = Quaternion.identity;
                if (VRInstanceManager.firstPerson)
                    tempCamHolder.transform.position = MenuManager.Instance.transform.root.position + (MenuManager.Instance.transform.root.up * 0.4f) +(MenuManager.Instance.transform.root.forward * -0.2f);
                else
                    tempCamHolder.transform.position = MenuManager.Instance.transform.root.position + (MenuManager.Instance.transform.root.forward * -1.5f);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuManager), "BackToMainMenu")]
        private static void PositionCamOnReturnToMenu(MenuManager __instance)
        {
            Logs.WriteWarning("RETURNING TO MENU");
            VRInstanceManager.camRoot.transform.SetParent(null);
            __instance.transform.SetParent(null);
            Logs.WriteWarning("DEPARENTING");
            //__instance.transform.parent.DetachChildren();
            loadingCamHolder.active = true;
            tempCamHolder.transform.position = new Vector3(-3.5f, -1.25f, -0.7861f);
            tempCamHolder.transform.rotation = Quaternion.identity;
            Camera loadingCam = loadingCamHolder.GetComponent<Camera>();
            loadingCam.cullingMask = 32;
            loadingCam.clearFlags = CameraClearFlags.SolidColor;
            loadingCam.backgroundColor = Color.black;
            loadingCam.nearClipPlane = CameraHandler.NEAR_CLIP_PLANE_VALUE;
            loadingCam.depth = 10;
            __instance.transform.position = tempCamHolder.transform.position + (tempCamHolder.transform.right * -0.5f) + (tempCamHolder.transform.up * 1.25f) + (tempCamHolder.transform.forward * 5f);
            __instance.transform.localRotation = Quaternion.identity;
            __instance.transform.rotation = Quaternion.identity;

        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(PauseMenu), "AwakeInit")]
        private static void AddVRSettings(PauseMenu __instance)
        {
            if (__instance.m_btnTogglePause.transform.parent.childCount == 2) {
                //if (__instance.transform.GetChild(0).name == "Blackscreen")
                //    __instance.transform.GetChild(0).gameObject.active = false;

                GameObject toggleBobButton = Object.Instantiate(__instance.m_btnTogglePause.gameObject);
                toggleBobButton.transform.parent = __instance.m_btnTogglePause.transform.parent;
                toggleBobButton.transform.localPosition = new Vector3(0,-195,0);
                toggleBobButton.transform.GetChild(0).GetComponent<Text>().text = "Allow headbob: " + ((VRInstanceManager.headBobOn) ? "On" : "Off");
                Button buttonComp = toggleBobButton.GetComponent<Button>();
                buttonComp.onClick = new Button.ButtonClickedEvent();
                buttonComp.onClick.AddListener(() => HeadbobButtonHandler(toggleBobButton.transform.GetChild(0).GetComponent<Text>()));
                
                GameObject combatFreezeButton = Object.Instantiate(__instance.m_btnTogglePause.gameObject);
                combatFreezeButton.transform.parent = __instance.m_btnTogglePause.transform.parent;
                combatFreezeButton.transform.localPosition = new Vector3(0, -245, 0);
                combatFreezeButton.transform.GetChild(0).GetComponent<Text>().text = "Move during attack: " + ((VRInstanceManager.freezeCombat) ? "Off" : "On");
                buttonComp = combatFreezeButton.GetComponent<Button>();
                buttonComp.onClick = new Button.ButtonClickedEvent();
                buttonComp.onClick.AddListener(() => FreezeCombatButtonHandler(combatFreezeButton.transform.GetChild(0).GetComponent<Text>()));

                GameObject moveDirectionButton = Object.Instantiate(__instance.m_btnTogglePause.gameObject);
                moveDirectionButton.transform.parent = __instance.m_btnTogglePause.transform.parent;
                moveDirectionButton.transform.localPosition = new Vector3(0, -295, 0);
                moveDirectionButton.transform.GetChild(0).GetComponent<Text>().text = "Move direction: " + (VRInstanceManager.moveFromHeadset ? "Headset" : "Hand");
                buttonComp = moveDirectionButton.GetComponent<Button>();
                buttonComp.onClick = new Button.ButtonClickedEvent();
                buttonComp.onClick.AddListener(() => MoveDirectionButtonHandler(moveDirectionButton.transform.GetChild(0).GetComponent<Text>()));

                GameObject background = toggleBobButton.transform.parent.parent.GetChild(1).gameObject;
                if (background.name == "BG") {
                    background.transform.localScale = new Vector3(1, 1.3f, 1);
                    background.transform.localPosition = new Vector3(0, -75, 0);
                }
            }
        }


        private static void HeadbobButtonHandler(Text buttonText)
        {
            VRInstanceManager.ToggleHeadBob();
            buttonText.text = "Allow headbob: " + ((VRInstanceManager.headBobOn) ? "On" : "Off");
        }

        private static void FreezeCombatButtonHandler(Text buttonText)
        {
            VRInstanceManager.freezeCombat = !VRInstanceManager.freezeCombat;
            buttonText.text = "Move during attack: " + ((VRInstanceManager.freezeCombat) ? "Off" : "On");
        }

        private static void MoveDirectionButtonHandler(Text buttonText)
        {
            VRInstanceManager.moveFromHeadset = !VRInstanceManager.moveFromHeadset;
            buttonText.text = "Move direction: " + (VRInstanceManager.moveFromHeadset ? "Headset" : "Hand");
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
                tempCamHolder.transform.localRotation = Quaternion.identity;
                __instance.transform.localRotation = Quaternion.identity;
                __instance.transform.position = tempCamHolder.transform.position + (tempCamHolder.transform.right * -0.5f) + (tempCamHolder.transform.up * 1.25f) + (tempCamHolder.transform.forward * 5f);
                Camera loadingCam = loadingCamHolder.GetComponent<Camera>();
                loadingCam.cullingMask = 32;
                loadingCam.clearFlags = CameraClearFlags.SolidColor;
                loadingCam.backgroundColor = Color.black;
                loadingCam.nearClipPlane = CameraHandler.NEAR_CLIP_PLANE_VALUE;
                loadingCam.depth = 10;
                Transform GeneralMenus = __instance.m_masterLoading.transform.parent;
                GeneralMenus.rotation = Quaternion.identity;
                GeneralMenus.localRotation = Quaternion.identity;
            }

            VRInstanceManager.isLoading = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterCreationPanel), "PutBackCamera")]
        private static void ReturnCameraFromCharacterCreation(CharacterCreationPanel __instance)
        {
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

            Transform GeneralMenus = __instance.CharacterUI.transform.root.GetChild(2);
            if (GeneralMenus.name == "GeneralMenus")
            {
                GeneralMenus.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
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
            tempCamHolder.transform.position = new Vector3(-3.75f, -1.25f, -3f);
            tempCamHolder.transform.rotation = Quaternion.identity;
            if (__instance.CharacterUI != null)
                __instance.CharacterUI.gameObject.active = false;
        }


        private static void PositionMenuManager(GameObject menuManager)
        {
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
                    menuManager.transform.position = new Vector3(-5.3117f, 1f, 2.1f);
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
            menuCanvas.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            menuCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
            menuCanvas.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);

            menuCanvas.transform.root.localScale = new Vector3(0.005f, 0.005f, 0.005f);

            mainCam.transform.parent = tempCamHolder.transform;
            mainCam.cullingMask = -1;
            mainCam.nearClipPlane = CameraHandler.NEAR_CLIP_PLANE_VALUE;
            mainCam.targetTexture = null;
            mainCam.gameObject.AddComponent<SteamVR_TrackedObject>();


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

            menuManager = __instance.transform.root.gameObject;
            PositionMenuManager(menuManager);
            tempCamHolder.transform.Rotate(0, -25, 0);
            Button firstMenuButton = __instance.FirstSelectable.GetComponent<Button>();
            if (firstMenuButton != null)
                firstMenuButton.Select();
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainScreen), "Update")]
        private static void UpdateControllersOnMainMenu(MainScreen __instance)
        {
            Controllers.Update();
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(TitleScreenLoader), "LoadTitleScreen")]
        private static void PositionDifferentMenus(TitleScreenLoader __instance, object[] __args)
        {
            chosenTitleScreen = __args[0] as string;
            Logs.WriteWarning("LOADING TITLE SCREEN");
            // When returning from a game to the main menu, the main menu FirstUpdate gets ran before this so it tries to position the
            // menu manager based on the title screen from when the game first loaded, so we need to re-load it here
            if (VRInstanceManager.gameHasBeenLoadedOnce)
            {
                PositionMenuManager(menuManager);
            }
        }
    }
}
