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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuManager), "Update")]
        private static void CharacterCamera_Update(MenuManager __instance, RectTransform ___m_characterUIHolder)
        {

            // I find these values work nicely for positioning the HUD
            LocalCharacterControl characterController = Camera.main.transform.root.GetComponent<LocalCharacterControl>();
            if (characterController != null && characterController.Character.Sneaking)
                __instance.transform.position = characterController.transform.position + (characterController.transform.right * 0.05f) + (characterController.transform.forward * 0.7f) + (characterController.transform.up * 1.2f);
            else
                __instance.transform.position = characterController.transform.position + (characterController.transform.forward * 0.6f) + (characterController.transform.up * 1.675f);
            if (characterController.Character.Sprinting)
                __instance.transform.position += (characterController.transform.forward * 0.2f);

            //__instance.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * 0.5f) + (Camera.main.transform.right * -0.05f) + (Camera.main.transform.up * 0.05f);
            __instance.transform.rotation = characterController.transform.rotation;
        }



        //private static void SetupUIShader()
        //{
        //    // Load bundle
        //    var bundle = AssetBundle.LoadFromFile(ASSETBUNDLE_PATH);
        //    AlwaysOnTopMaterial = bundle.LoadAsset<Material>("UI_AlwaysOnTop");

        //    //// Fix loaded images and text

        //    //FixUIMaterials(Resources.FindObjectsOfTypeAll<Image>(),
        //    //               Resources.FindObjectsOfTypeAll<Text>());

        //    //// Fix UIUtilities prefabs

        //    //var prefabs = new MonoBehaviour[] { UIUtilities.ItemDisplayPrefab, UIUtilities.ItemDetailPanel };
        //    //foreach (var obj in prefabs)
        //    //{
        //    //    FixUIMaterials(obj.GetComponentsInChildren<Image>(true),
        //    //                   obj.GetComponentsInChildren<Text>(true));
        //    //}
        //}

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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ItemDisplayClick), "RightClick")]
        public static void PositwionBandwwage(ItemDisplayClick __instance, object[] __args)
        {
            //Logs.WriteInfo(__args[0]);
           
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Panel), "OnShowInvokeFocus")]
        public static void PositwionBanwdwwage(Panel __instance)
        {
            Logs.WriteInfo("FOCUS");

        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterUI), "ToggleMenu")]
        public static void PositwionBanwdwwwage(Panel __instance, object[] __args)
        {
            
            //CharacterUI.MenuScreens t = __args[0] as CharacterUI.MenuScreens;
            Logs.WriteInfo("Toggle");

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EventTrigger), "OnPointerEnter")]
        public static void FixContexteMenu(EventTrigger __instance, object[] __args)
        {
            //PointerEventData t = __args[0] as PointerEventData;
            //Logs.WriteWarning(t);
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterUI), "get_EventSystemCurrentSelectedGo")]
        //public static void FixContedxtMenu(CharacterUI __instance, ref GameObject __result)
        public static void FixContedxtMenu(CharacterUI __instance, ref GameObject __result)
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


        //    [HarmonyPostfix]
        //[HarmonyPatch(typeof(ContextualMenu), "Show")]
        //public static void FixContextMenu(ContextualMenu __instance)
        //{


        //    //___m_optionButtonTemplate.gameObject.SetActive(true);
        //    Transform contextMenu = __instance.gameObject.transform.GetChild(0);
        //    //Logs.WriteWarning(contextMenu.gameObject.name);

        //    //EventSystem.current.SetSelectedGameObject(__instance.gameObject.transform.GetChild(0).gameObject, null);
        //    contextMenu.GetChild(contextMenu.childCount - 1).GetChild(0).gameObject.SetActive(true);
        //    contextMenu.GetChild(contextMenu.childCount - 1).gameObject.SetActive(true);
        //    //contextMenu.GetChild(contextMenu.childCount - 1).GetComponent<Button>().Select();
        //    test = contextMenu.GetChild(contextMenu.childCount - 1).GetComponent<Button>();
        //    __instance.CharacterUI.GetType().GetField("m_currentSelectedGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(__instance.CharacterUI, contextMenu.GetChild(contextMenu.childCount - 1).gameObject);
        //    //for (int i = 0; i < contextMenu.childCount; i++)
        //    //{
        //    //    if (contextMenu.GetChild(i).gameObject.name != "Background" && contextMenu.GetChild(i).gameObject.GetActive())
        //    //    {

        //    //        PointerEventData _data = new PointerEventData(EventSystem.current);
        //    //        _data.pointerEnter = contextMenu.GetChild(i).gameObject;
        //    //        _data.position = new Vector2(1203f, 1108f);
        //    //        contextMenu.GetChild(i).gameObject.GetComponent<Button>().OnPointerEnter(_data);
        //    //        Logs.WriteWarning("Triggered enter");
        //    //        contextMenu.GetChild(i).gameObject.SetActive(true);
        //    //        contextMenu.GetChild(i).gameObject.GetComponent<Button>().SetDownNav(contextMenu.GetChild(i+i).gameObject.GetComponent<Button>());
        //    //        test = contextMenu.GetChild(i).gameObject.GetComponent<Button>();


        //    //        EventSystem.current.SetSelectedGameObject(null);
        //    //        EventSystem.current.SetSelectedGameObject(contextMenu.GetChild(i).gameObject);

        //    //        //PointerEventData _data = new PointerEventData(EventSystem.current);
        //    //        //_data.pointerPress = UI.invItem.gameObject;
        //    //        //_data.position = UI.invItem.gameObject.transform.position;
        //    //        //_data.position = new Vector2(1019f, 1143f);
        //    //        //Logs.WriteWarning(contextMenu.GetChild(i).gameObject.name);
        //    //        //contextMenu.GetChild(i).GetComponent<Button>().Select();
        //    //        //i = contextMenu.childCount;
        //    //    }
        //    //}

        //}




        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(ContextualMenu), "Show")]
        //public static void FixContextMenu(ContextualMenu __instance, object[] __args, 
        //    UnityEngine.Transform ___m_optionButtonTemplate,
        //    int ___m_itemCount,
        //    bool[] ___m_flippedAxis,
        //    RectTransform ___contentRectTransform,
        //    GameObject ___m_previouslySelectedObject,

        //    List<Button> ___m_actionButtons
        //    )
        //{
        //    List<KeyValuePair<string, UnityAction>> _options = __args[1] as List<KeyValuePair<string, UnityAction>>;
        //    if (!___m_optionButtonTemplate || _options.Count <= 0)
        //    {
        //        return;
        //    }
        //    ___m_itemCount = _options.Count;
        //    for (int i = 0; i < ___m_flippedAxis.Length; i++)
        //    {
        //        if (___m_flippedAxis[i])
        //        {
        //            ___m_flippedAxis[i] = false;
        //            RectTransformUtility.FlipLayoutOnAxis(___contentRectTransform, i, false, false);
        //        }
        //    }
        //    m_previouslySelectedObject = m_characterUI.CurrentSelectedGameObject;
        //    if ((bool)___m_optionButtonTemplate)
        //    {
        //        ___m_optionButtonTemplate.gameObject.SetActive(value: true);
        //    }
        //    for (int j = 0; j < _options.Count; j++)
        //    {
        //        if (j >= ___m_actionButtons.Count)
        //        {
        //            Transform transform = Object.Instantiate(___m_optionButtonTemplate);
        //            transform.SetParent(m_panel.transform);
        //            transform.ResetLocal();
        //            ___m_actionButtons.Add(transform.GetComponent<Button>());
        //        }
        //        ___m_actionButtons[j].gameObject.SetActive(value: true);
        //        ___m_actionButtons[j].GetComponentInChildren<Text>().text = _options[j].Key;
        //        ___m_actionButtons[j].onClick.RemoveAllListeners();
        //        ___m_actionButtons[j].onClick.AddListener(_options[j].Value);
        //    }
        //    for (int k = 0; k < ___m_actionButtons.Count; k++)
        //    {
        //        if (k < _options.Count)
        //        {
        //            if (k > 0)
        //            {
        //                ___m_actionButtons[k].SetUpNav(___m_actionButtons[k - 1]);
        //            }
        //            if (k < _options.Count - 1)
        //            {
        //                ___m_actionButtons[k].SetDownNav(___m_actionButtons[k + 1]);
        //            }
        //            if (k == 0)
        //            {
        //                ___m_actionButtons[k].SetUpNav(___m_actionButtons[_options.Count - 1]);
        //            }
        //            if (k == _options.Count - 1)
        //            {
        //                ___m_actionButtons[k].SetDownNav(___m_actionButtons[0]);
        //            }
        //        }
        //        else
        //        {
        //            ___m_actionButtons[k].gameObject.SetActive(value: false);
        //        }
        //    }
        //    Show();
        //    m_canvasGroup.set_blocksRaycasts(true);
        //    m_globalMousePos = Vector3.zero;
        //    RectTransform obj = base.transform as RectTransform;
        //    Vector2 vector = _data?.position ?? Vector2.zero;
        //    if (RectTransformUtility.ScreenPointToWorldPointInRectangle(obj, vector, _data?.pressEventCamera, ref m_globalMousePos))
        //    {
        //        RectTransform obj2 = ___m_optionButtonTemplate.transform as RectTransform;
        //        obj2.gameObject.SetActive(value: true);
        //        Vector2 size = obj2.rect.size;
        //        Vector2 sizeDelta = contentRectTransform.sizeDelta;
        //        sizeDelta.y = size.y * (float)m_itemCount;
        //        contentRectTransform.sizeDelta = sizeDelta;
        //        m_panel.transform.position = m_globalMousePos;
        //        Vector3[] array = new Vector3[4];
        //        contentRectTransform.GetWorldCorners(array);
        //        RectTransform rectTransform = m_characterUI.UIPanel.transform as RectTransform;
        //        Rect rect = rectTransform.rect;
        //        for (int l = 0; l < 2; l++)
        //        {
        //            bool flag = false;
        //            for (int m = 0; m < 4; m++)
        //            {
        //                Vector3 vector2 = rectTransform.InverseTransformPoint(array[m]);
        //                if (vector2[l] < rect.min[l] || vector2[l] > rect.max[l])
        //                {
        //                    flag = true;
        //                    break;
        //                }
        //            }
        //            if (flag)
        //            {
        //                RectTransformUtility.FlipLayoutOnAxis(contentRectTransform, l, false, false);
        //                m_flippedAxis[l] = true;
        //            }
        //        }
        //        m_panel.transform.position = m_globalMousePos;
        //    }
        //    if ((bool)___m_optionButtonTemplate)
        //    {
        //        ___m_optionButtonTemplate.gameObject.SetActive(value: false);
        //    }
        //}


        [HarmonyPrefix]
        [HarmonyPatch(typeof(PointerEventData), "get_pressEventCamera")]
        public static bool PositwionBwandwwage(PointerEventData __instance, ref Camera __result)
        {
            __result = Camera.main;
            return false;
        }
        public static UnityEngine.UI.Button test;
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
