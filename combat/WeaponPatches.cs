using HarmonyLib;
using OutwardVR.body;
using OutwardVR.camera;
using UnityEngine;

namespace OutwardVR.combat
{
    [HarmonyPatch]
    internal class WeaponPatches
    {
        public static bool hitWhileBlocking = false;


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Weapon), "OnUnequip")]
        private static void ResetArmIKOnUnequip(Weapon __instance) {
            if (!VRInstanceManager.firstPerson || !__instance.OwnerCharacter.IsLocalPlayer || !NetworkLevelLoader.Instance.IsOverallLoadingDone)
                return;
            // Only run this if nothing is being equipped to replace it
            if (VRInstanceManager.leftHandIK != null && __instance.OwnerCharacter.CurrentWeapon == null)
                VRInstanceManager.leftHandIK.ChangeWeapon(false);

        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(Weapon), "OnEquip")]
        private static void AddVRToWeaponOnEquip(Weapon __instance, object[] __args)
        {
            // Weapon Types:
            //Sword_1H = 0,
            //Axe_1H = 1,
            //Mace_1H = 2,
            //Dagger_OH = 30,
            //Chakram_OH = 40,
            //Pistol_OH = 45,
            //Halberd_2H = 50,
            //Sword_2H = 51,
            //Axe_2H = 52,
            //Mace_2H = 53,
            //Spear_2H = 54,
            //FistW_2H = 55,
            //Shield = 100,
            //Arrow = 150,
            //Bow = 200

            if (!VRInstanceManager.firstPerson || !__instance.OwnerCharacter.IsLocalPlayer || !NetworkLevelLoader.Instance.IsOverallLoadingDone)
                return;
            if (VRInstanceManager.leftHandIK != null && __instance.TwoHanded && __instance.TwoHand != Equipment.TwoHandedType.DualWield && __instance.Type != Weapon.WeaponType.Bow)
                VRInstanceManager.leftHandIK.ChangeWeapon(true);
            else if (VRInstanceManager.leftHandIK != null)
                VRInstanceManager.leftHandIK.ChangeWeapon(false);
            if (__instance.CurrentVisual.GetComponent<VRMeleeHandler>() == null) {
                if (__instance.Type != Weapon.WeaponType.Pistol_OH &&
                    __instance.Type != Weapon.WeaponType.Shield &&
                    __instance.Type != Weapon.WeaponType.Arrow &&
                    __instance.Type != Weapon.WeaponType.Bow &&
                     __instance.Type != Weapon.WeaponType.Chakram_OH &&
                      __instance.Type != Weapon.WeaponType.FistW_2H
                    ) {
                    __instance.CurrentVisual.gameObject.AddComponent<VRMeleeHandler>();
                }
            }
            if (__instance.CurrentVisual.GetComponent<VRFisticuffsHandler>() == null && __instance.Type == Weapon.WeaponType.FistW_2H)
                __instance.CurrentVisual.gameObject.AddComponent<VRFisticuffsHandler>();
            if (__instance.CurrentVisual.GetComponent<VRShieldHandler>() == null && __instance.Type == Weapon.WeaponType.Shield) 
                __instance.CurrentVisual.gameObject.AddComponent<VRShieldHandler>();

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Weapon), "HasBlocked")]
        private static void SetPlayerHasBlocked(Weapon __instance, object[] __args) {
            if (__instance.OwnerCharacter.IsLocalPlayer) 
                hitWhileBlocking = true;
        }

    

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "SendPerformAttackTrivial", new[] { typeof(int), typeof(int), typeof(bool) })]
        private static bool PerformAttackWithoutAnimation(Character __instance, object[] __args)
        {
            if (!VRInstanceManager.firstPerson || 
                !__instance.IsLocalPlayer ||
                (int)__args[0] >= 2 || 
                __instance.CurrentWeapon == null ||
                __instance.CurrentWeapon.Type == Weapon.WeaponType.Pistol_OH ||
                __instance.CurrentWeapon.Type == Weapon.WeaponType.Shield ||
                __instance.CurrentWeapon.Type == Weapon.WeaponType.Arrow ||
                __instance.CurrentWeapon.Type == Weapon.WeaponType.Bow ||
                __instance.CurrentWeapon.Type == Weapon.WeaponType.Chakram_OH)
                return true;
            __instance.SetHitTrans(-1);
            __instance.StopBlocking();
            __instance.m_attackType = (int)__args[0];
            __instance.m_attackID = (int)__args[1];
            if (!__instance.photonView.isMine && __instance.m_sheathed)
            {
                __instance.MakeSureEnabled();
                __instance.SendSheathe(_sheathed: true, _instant: true);
            }
            if (__instance.m_characterSoundManager != null)
            {
                Global.AudioManager.PlaySoundAtPosition(__instance.m_characterSoundManager.GetAttackSound((int)__args[0]), __instance.transform);
            }
            __instance.m_animator.SetInteger("AttackID", __instance.m_attackID);
            __instance.SendMessage("PerformAttack", SendMessageOptions.DontRequireReceiver);
            __instance.m_blockDesired = false;
            __instance.m_blocking = false;
            __instance.NotifyNearbyAIOfAttack();
            __instance.m_stackedDetectability += 10f;
            __instance.m_stackedVisualDetectability += 10f;
            return false;
        }

    }
}
