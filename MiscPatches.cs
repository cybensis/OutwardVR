using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using static UnityEngine.UIElements.UIR.BestFitAllocator;
using Valve.VR;
using NodeCanvas.Framework;

namespace OutwardVR
{
    [HarmonyPatch]
    internal class MiscPatches
    {
        public static CharacterUI characterUIInstance;


        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterUI), "Awake")]
        private static void SetUIInstance(CharacterUI __instance)
        {
            characterUIInstance = __instance;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterVisuals), "EquipVisuals")]
        private static void DisableHelmet(CharacterVisuals __instance)
        {

            if (!VRInstanceManager.firstPerson || !__instance.m_character.IsLocalPlayer)
                return;
            try
            {
                if (__instance.ActiveVisualsHelmOrHead != null && __instance.ActiveVisualsHelmOrHead.Renderer != null) {
                    VRInstanceManager.activeVisualsHelmOrHead = __instance.ActiveVisualsHelmOrHead.Renderer;
                    VRInstanceManager.activeVisualsHelmOrHead.enabled = false;
                }
                if (__instance.DefaultHairVisuals != null && __instance.DefaultHairVisuals.gameObject != null) {
                    VRInstanceManager.playerHair = __instance.DefaultHairVisuals.GetComponent<SkinnedMeshRenderer>();
                    VRInstanceManager.playerHair.enabled = false;
                }
            }
            catch {
                return;
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(DialoguePanel), "Update")]
        private static void TuteFix(DialoguePanel __instance)
        {
            if (!DemoManager.DemoIsActive)
                return;

            string tuteName = __instance.m_lastNpcLocKey;
            if (tuteName == "Player_Message_TutorialTitle_Attacks")
                __instance.m_lblNpcSpeech.text = "Perform attacks by swinging your controller around or for non-fist weapons, stab attack can also be used";
            else if (tuteName == "Player_Message_TutorialTitle_Blocking")
                __instance.m_lblNpcSpeech.text = @"Blocking can either be done with <color=#D6A260>[RightGrip]</color> or using the motion controls to prevent the damage from a strike and reduces how much you would be knocked back. 
To block with a weapon, turn it to the side so the tip of the weapon is facing to the left or right of the player, for shields simply hold it out in front of you as you would an actual shield, and for fist type weapons, holding both
hands up in front of your face with your fists pointing to the sky will activate blocking.

Shields consume less stamina when blocking and can block arrows.";
        }




        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapMarkerSimpleDisplay), "Update")]
        private static void FixMapMarkerCursor(MapMarkerSimpleDisplay __instance)
        {
            bool isHovering = Vector2.Distance(new Vector2(__instance.RectTransform.localPosition.x, __instance.RectTransform.localPosition.y), MapDisplay.Instance.ControllerCursor.anchoredPosition) <= 20;
            if (!__instance.m_hover && isHovering)
                __instance.OnPointerEnter(null);
            else if (__instance.m_hover && !isHovering)
                __instance.OnPointerExit(null);
            
            if (isHovering && SteamVR_Actions._default.ButtonA.stateDown) {
                PointerEventData pointerData = new PointerEventData(EventSystem.current);
                __instance.OnPointerClick(pointerData);
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(RadialSelectorItem), "Update")]
        private static void ActivateRadialItem(RadialSelectorItem __instance)
        {
            // Definitely can find a more efficient method of doing this but since its only active when the map is open its not that big a deal

            // Get the tip of the arrow selector
            Vector3 tipOfSelector = __instance.Selector.ArrowTrans.position + (__instance.Selector.ArrowTrans.transform.up * __instance.Selector.ArrowTrans.transform.localScale.y / 2);

            // Position of the second object
            Vector3 marker = __instance.transform.position;
            bool isPointedAt = (Vector3.Distance(tipOfSelector, marker) <= 0.4765);


            if (!__instance.m_hover && isPointedAt)
                __instance.OnPointerEnter(null);
            else if (__instance.m_hover && !isPointedAt)
                __instance.OnPointerExit(null);

            if (__instance.m_hover && SteamVR_Actions._default.ButtonA.stateDown)
                __instance.OnPointerClick(null);
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapDisplay), "Update")]
        private static void EnableBackgroundClick(MapDisplay __instance) {
            if (__instance.HoveredMarker == null && SteamVR_Actions._default.ButtonA.stateDown)
                __instance.OnMapBackgroundClicked(true);

        }

        private static float radialLastDirection = 0;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadialSelector), "Update")]
        private static bool FixRadialMenu(RadialSelector __instance)
        {

            if (!__instance.TargetActive)
            {
                __instance.m_alpha = Mathf.MoveTowards(__instance.m_alpha, 0f, __instance.ActiveAnimSpeed * Time.deltaTime);
                if (__instance.m_alpha == 0f)
                {
                    __instance.gameObject.SetActive(value: false);
                }
                __instance.m_canvasGroup.alpha = __instance.m_alpha;
            }
            else if (__instance.m_alpha != 1f)
            {
                __instance.m_alpha = Mathf.MoveTowards(__instance.m_alpha, 1f, __instance.ActiveAnimSpeed * Time.deltaTime);
                __instance.m_canvasGroup.alpha = __instance.m_alpha;
            }
            Vector3 targetDir = __instance.m_characterUI.VirtualCursor.RectTransform.position - __instance.transform.position;
            //float z = __instance.transform.up.AngleWithDir(targetDir, __instance.transform.forward);
            float z = radialLastDirection;
            if (SteamVR_Actions._default.LeftJoystick.axis.magnitude != 0) { 
                z = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
                z -= 90;
                if (z < 0)
                    z += 360;
                radialLastDirection = z;
            }
            if ((bool)__instance.ArrowTrans && (bool)(Object)(object)__instance.ArrowCanvas)
            {
                __instance.ArrowTrans.ResetLocal();
                __instance.ArrowTrans.Rotate(new Vector3(0f, 0f, z), Space.Self);
            }
            if (ControlsInput.IsLastActionGamepad(__instance.PlayerID))
            {
                Vector3 vector = new Vector3(ControlsInput.MenuHorizontalAxis(__instance.PlayerID), ControlsInput.MenuVerticalAxis(__instance.PlayerID), 0f);
                __instance.m_characterUI.VirtualCursor.SetPosition(__instance.transform.position + vector.normalized * __instance.Radius);
            }
            __instance.ArrowCanvas.alpha = 1f;
            return false;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(MapDisplay), "OnMapBackgroundClicked")]
        private static bool FixMapBackgroundClick(MapDisplay __instance, object[] __args)
        {
            bool _left = (bool)__args[0];
            if (!__instance.m_currentAreaHasMap || __instance.HoveredMarker)
            {
                return false;
            }
            if (!__instance.m_markerSelector.TargetActive)
            {
                bool flag = ControlsInput.IsLastActionGamepad(__instance.PlayerID);
                if ((_left == flag) ? true : false)
                {
                    __instance.m_markerSelector.transform.position = MapDisplay.Instance.ControllerCursor.position;
                    __instance.m_markerSelector.transform.localPosition = MapDisplay.Instance.ControllerCursor.anchoredPosition;
                    __instance.m_markerSelector.SetActiveWithAnim(_active: true);
                }
            }
            else
            {
                __instance.m_markerSelector.SetActiveWithAnim(_active: false);
            }
            return false;
        }





        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainScreen), "Update")]
        private static void UpdateControllersOnMainMenu(MainScreen __instance)
        {
            Controllers.Update();
        }



        [HarmonyPrefix]
        [HarmonyPatch(typeof(ControlsInput), "IsLastActionGamepad")]
        private static bool SetUsingGamepad(ref bool __result)
        {
            __result = true;
            return false;
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


        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterJointManager), "Start")]
        private static void SetHeadJoint(CharacterJointManager __instance)
        {
            if (__instance.name == "head" && __instance.transform.root.name != "AISquadManagerStructure")
                VRInstanceManager.modelPlayerHead = __instance.transform.gameObject;
            if (__instance.name == "hand_left" && __instance.transform.root.name != "AISquadManagerStructure")
                VRInstanceManager.modelLeftHand = __instance.transform.gameObject;
            if (__instance.name == "hand_right" && __instance.transform.root.name != "AISquadManagerStructure")
                VRInstanceManager.modelRightHand = __instance.transform.gameObject;
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
                        //__instance.GetType().GetField("m_currentSelectedGameObject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(__instance, contextButton);
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



        [HarmonyPrefix]
        [HarmonyPatch(typeof(PointerEventData), "get_pressEventCamera")]
        // Pretty sure this is used for activating the inventory context menu because we need to create a fake pointer event when activating the
        // context menu but can't set the camera manually, so we need to do that here.
        private static bool SetCamOnPressEvent(PointerEventData __instance, ref Camera __result)
        {
            __result = Camera.main;
            return false;
        }

    }
}
