using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
            if (!__instance.m_character.IsLocalPlayer)
                return;
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
