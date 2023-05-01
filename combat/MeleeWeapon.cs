using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static EffectSynchronizer;
using static FightRecap;

namespace OutwardVR.combat
{
    [HarmonyPatch]
    internal class MeleeWeapon
    {

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(Weapon), "HasHit")]
        //private static bool dddwd(Weapon __instance, object[] __args) {
        //    RaycastHit _hit = (RaycastHit)__args[0];
        //    Vector3 _dir = (Vector3)__args[1];
        //    Hitbox component = _hit.collider.GetComponent<Hitbox>();
        //    Logs.WriteWarning("PRE");
        //    if (!__instance.ElligibleFaction(component) || __instance.m_alreadyHitChars.Contains(component.OwnerChar))
        //    {
        //        return false;
        //    }
        //            Logs.WriteWarning("POST");
        //    bool flag = false;
        //    float num = Vector3.Angle(component.OwnerChar.transform.forward, __instance.m_ownerCharacter.transform.position - component.OwnerChar.transform.position);
        //    float angleDir = _dir.AngleDir(component.OwnerChar.transform.forward, Vector3.up);
        //    if (!__instance.Unblockable && component.OwnerChar.Blocking && num < (float)(component.OwnerChar.ShieldEquipped ? Weapon.SHIELD_BLOCK_ANGLE : Weapon.BLOCK_ANGLE))
        //    {
        //        flag = true;
        //    }
        //    __instance.m_alreadyHitChars.Add(component.OwnerChar);
        //    if (__instance.m_lastDealtDamages != null)
        //    {
        //        __instance.m_lastDealtDamages.Clear();
        //    }
        //    DamageList damage = __instance.Damage;
        //    float num2 = __instance.Impact;
        //    if (__instance.m_attackID >= 0)
        //    {
        //        damage = __instance.GetDamage(__instance.m_attackID);
        //        num2 = __instance.GetKnockback(__instance.m_attackID);
        //        if (!flag)
        //        {
        //            __instance.m_lastDealtDamages = component.OwnerChar.ReceiveHit(__instance, damage, _dir, _hit.point, num, angleDir, __instance.m_ownerCharacter, num2);
        //        }
        //        else
        //        {
        //            component.OwnerChar.ReceiveBlock(__instance, damage, _dir, num, angleDir, __instance.m_ownerCharacter, num2);
        //        }
        //    }
        //    if ((bool)__instance.OwnerCharacter)
        //    {
        //        __instance.OwnerCharacter.HasHit(__instance, damage.TotalDamage, _dir, _hit.point, num, flag, component.OwnerChar, num2, __instance.m_attackID);
        //    }
        //    if (__instance is MeleeWeapon)
        //    {
        //        __instance.ReduceDurability(Weapon.IsSpecialAttack(__instance.m_attackID) ? 1.5f : 1f);
        //    }
        //    if (CharacterManager.Instance.IsFightTrackingActive)
        //    {
        //        CharacterManager.Instance.FightTracking.HasHit(__instance.m_ownerCharacter, component.OwnerChar, __instance, __instance.m_attackID, (__instance.m_attackID != -1) ? __instance.GetDamage(__instance.m_attackID) : __instance.Damage, damage, flag);
        //    }
        //    if (__instance.m_attackID >= 0)
        //    {
        //        __instance.SynchronizeEffects(EffectCategories.Hit, (!flag) ? component.OwnerChar : null, _hit.point, _dir);
        //    }
        //    object[] parameter = new object[2] { _hit, _dir };
        //    if ((bool)__instance.EquippedVisuals)
        //    {
        //        __instance.EquippedVisuals.BroadcastMessage("OnHasHit", parameter, SendMessageOptions.DontRequireReceiver);
        //    }
        //    if (__instance.m_lastDealtDamages.TotalDamage > 0f)
        //    {
        //        __instance.ProcessDamageDealt(__instance.m_lastDealtDamages);
        //    }
        //    return false;
        //}

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "SendPerformAttackTrivial", new[] { typeof(int), typeof(int), typeof(bool) })]
        private static bool dd(Character __instance, object[] __args)
        {
            if (!__instance.IsLocalPlayer)
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
            //if (__instance.m_attackType < 2)
            //{
            //    if (__instance.CanTiredAttack)
            //    {
            //        if ((bool)__args[2])
            //        {
            //            __instance.m_animator.SetBool("TiredAttack", value: true);
            //        }
            //        else
            //        {
            //            __instance.m_animator.SetBool("TiredAttack", value: false);
            //        }
            //    }
            //    if (__instance.m_currentWeapon != null)
            //    {
            //        float attackSpeed = __instance.m_currentWeapon.GetAttackSpeed();
            //        if (attackSpeed > 0f)
            //        {
            //            __instance.m_animator.SetFloat("AttackSpeed", attackSpeed);
            //        }
            //        else
            //        {
            //            __instance.m_animator.SetFloat("AttackSpeed", 1f);
            //        }
            //    }
            //    else
            //    {
            //        __instance.m_animator.SetFloat("AttackSpeed", 1f);
            //    }
            //    __instance.m_animator.SetTrigger(__instance.ATTACK_ANIM_TRIGGERS[__instance.m_attackType]);
            //    Logs.WriteWarning(__instance.ATTACK_ANIM_TRIGGERS[__instance.m_attackType]);
            //}
            //else
            //{
            //    __instance.m_animator.SetInteger("SpellType", __instance.m_attackType);
            //    __instance.m_animator.SetTrigger("Spell");
            //}
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
