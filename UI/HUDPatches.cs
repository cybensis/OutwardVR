using HarmonyLib;
using UnityEngine;
using Valve.VR;

namespace OutwardVR.UI
{
    [HarmonyPatch]
    internal class HUDPatches
    {
        private static GameObject statusBars;
        private static GameObject quickSlots;


        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuManager), "Update")]
        private static void PositionHUD(MenuManager __instance)
        {
            try
            {
                Character character = Camera.main.transform.root.GetComponent<Character>();

                if (character == null)
                    return;
                // By setting the HUD's parent to the head object it rotates with the body and by setting the local position, it is positioned perfectly
                if (__instance.transform.parent == null)
                {
                    __instance.transform.parent = Camera.main.transform.parent.parent.parent.transform;
                    __instance.transform.localRotation = Quaternion.identity;
                }

                __instance.transform.localPosition = new Vector3(-0.05f, 0.225f, 0.5f);
                //if (character.Sneaking)
                //    __instance.transform.localPosition += new Vector3(0, -0.4f, 0);
            }
            catch
            {
                return;
            }

        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(UICompass), "Update")]
        private static void FixCompassDirection(UICompass __instance)
        {
            if (Camera.main != null && __instance != null)
                __instance.TargetTransform = Camera.main.transform;
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
        [HarmonyPatch(typeof(QuestDisplay), "AwakeInit")]
        private static void FixQuestDisplayPosition(QuestDisplay __instance)
        {
            Vector3 newPos = __instance.transform.parent.localPosition;
            newPos.z = 0;
            __instance.transform.parent.localPosition = newPos;
        }


            [HarmonyPostfix]
        [HarmonyPatch(typeof(DeveloperToolManager), "Awake")]
        private static void PositionDebugToolWindow(DeveloperToolManager __instance)
        {
            __instance.transform.localPosition = new Vector3(-200f, 0f, 0f);
            __instance.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
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
    }
}
