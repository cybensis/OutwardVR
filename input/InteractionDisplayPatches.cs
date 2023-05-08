//using HarmonyLib;


//namespace OutwardVR.input
//{
//    [HarmonyPatch]
//    internal class InteractionDisplayPatches
//    {

//        public static LaserPointer laser;
//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(CharacterUI), "UpdateInteractDisplay")]
//        private static bool PatchUpdateInteractDisplay(CharacterUI __instance)
//        {
//            if (!NetworkLevelLoader.Instance.IsOverallLoadingDone || !NetworkLevelLoader.Instance.AllPlayerReadyToContinue)
//                return true;
//            if (!__instance.m_isInputReady || !(__instance.m_targetCharacter != null) || !__instance.m_interactionPanel.StartDone)
//            {
//                return false;
//            }
//            if (!__instance.IsMenuFocused && !__instance.IsDialogueInProgress)
//            {
//                if (laser == null)
//                {
//                    laser = __instance.m_targetCharacter.CurrentWeapon.CurrentVisual.transform.parent.parent.parent.GetComponent<LaserPointer>();
//                }
//                if (laser.worldItem != null)
//                {
//                    //Logs.WriteWarning("HIT " + laser.worldItem.transform.parent.parent.name);
//                    __instance.m_interactionPanel.SetInteractable(laser.worldItem.transform.parent.GetComponent<InteractionTriggerBase>());
//                    return false;
//                }
//                return true;
//            }
//            return true;
//        }




//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(Character), "get_Interactable")]
//        private static bool PatchInteractable (Character __instance, InteractionTriggerBase __result) {
//            if (laser != null && laser.worldItem != null) {
//                //Logs.WriteWarning("Patch int");
//                __result = laser.worldItem.transform.parent.GetComponent<InteractionTriggerBase>();
//                return false;
//            }
//            return true;
                
//        }




//        // Start looking into OnInteractButtonDown which starts a coroutine InteractButtonCoroutine which calls on the InteractionTriggerBase function TryActivateBasicAction
//        // Maybe just make pressing Y activate the ActivateBasicAction function



//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(InteractionDisplay), "SetInteractable")]
//        private static bool PatchSetInteractDisplay(InteractionDisplay __instance, object[] __args)
//        {
//            if (!NetworkLevelLoader.Instance.IsOverallLoadingDone || !NetworkLevelLoader.Instance.AllPlayerReadyToContinue)
//                return true;
//            if (laser == null || laser.worldItem == null)
//                return true;
//            InteractionTriggerBase _interactionTrigger = (InteractionTriggerBase)__args[0];
//            if (!__instance.m_startDone)
//            {
//                return false;
//            }
//            string pressText = _interactionTrigger.GetPressText(__instance.LocalCharacter);
//            string holdText = _interactionTrigger.GetHoldText(__instance.LocalCharacter);
//            bool flag = _interactionTrigger != null && (!string.IsNullOrEmpty(pressText) || !string.IsNullOrEmpty(holdText));
//            bool flag2 = (bool)_interactionTrigger && (bool)_interactionTrigger.ItemToPreview && _interactionTrigger.ItemToPreview is Bag;
//            flag |= flag2 && !__instance.LocalCharacter.IsHandlingBag;
//            if (__instance.IsDisplayed != flag)
//            {
//                __instance.Show(flag);
//            }
//            if (!flag)
//            {
//                return false;
//            }
//            __instance.SetValues(pressText, holdText);
//            __instance.InteractButton(__instance.LocalCharacter.IsHoldingInteract);
//            __instance.SetProgressRatio(__instance.LocalCharacter.InteractionCompleteRatio);
//            if ((bool)__instance.m_interactionBag && __instance.m_interactionBag.IsDisplayed != flag2)
//            {
//                __instance.m_interactionBag.Show(flag2);
//            }
//            if (_interactionTrigger.ShowItemPreview)
//            {
//                if ((bool)__instance.m_itemPreviewPanel)
//                {
//                    if (!__instance.m_itemPreviewPanel.IsDisplayed)
//                    {
//                        __instance.m_itemPreviewPanel.Show();
//                    }
//                    if ((bool)__instance.m_imgItemIcon)
//                    {
//                        __instance.m_imgItemIcon.overrideSprite = _interactionTrigger.ItemToPreview.ItemIcon;
//                    }
//                    if ((bool)__instance.m_imgBroken && __instance.m_imgBroken.gameObject.activeSelf != (_interactionTrigger.ItemToPreview.DurabilityRatio == 0f))
//                    {
//                        __instance.m_imgBroken.gameObject.SetActive(_interactionTrigger.ItemToPreview.DurabilityRatio == 0f);
//                    }
//                    if ((bool)__instance.m_imgEnchanted && __instance.m_imgEnchanted.gameObject.activeSelf != _interactionTrigger.ItemToPreview.IsEnchanted)
//                    {
//                        __instance.m_imgEnchanted.gameObject.SetActive(_interactionTrigger.ItemToPreview.IsEnchanted);
//                    }
//                    if ((bool)__instance.m_lblItemName)
//                    {
//                        __instance.m_lblItemName.text = _interactionTrigger.ItemToPreview.DisplayName + ((_interactionTrigger.ItemToPreview.IsStackable && _interactionTrigger.ItemToPreview.RemainingAmount > 1) ? (" (" + _interactionTrigger.ItemToPreview.RemainingAmount + ")") : "");
//                    }
//                    if ((bool)__instance.m_lblItemWeight)
//                    {
//                        __instance.m_lblItemWeight.text = _interactionTrigger.ItemToPreview.Weight.ToString("0.0");
//                    }
//                }
//            }
//            else if ((bool)__instance.m_itemPreviewPanel && __instance.m_itemPreviewPanel.IsDisplayed)
//            {
//                __instance.m_itemPreviewPanel.Hide();
//            }
//            return false;
//        }
//    }
//}
