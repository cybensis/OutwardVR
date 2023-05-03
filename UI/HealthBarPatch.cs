using HarmonyLib;
using UnityEngine;

namespace OutwardVR.UI
{
    [HarmonyPatch]
    internal class HealthBarPatch
    {

        private static GameObject enemyHealthHolder;

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
        }



        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterBarListener), "UpdateDisplay")]
        private static void HelpInitEnemyHealthBar(CharacterBarListener __instance)
        {
            // Since the health bar object no longer spawns in the MenuManager object it needs some
            // help initialising its values.
            if (__instance.gameObject.name == "CharacterBar(Clone)" && __instance.m_characterUI == null)
                __instance.m_characterUI = MiscPatches.characterUIInstance;
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CharacterBarListener), "UpdateDisplay")]
        private static void PositionEnemyHealth(CharacterBarListener __instance)
        {
            // This check for the name is because the player health bar also uses CharacterBarListener
            if (__instance.gameObject.name == "CharacterBar(Clone)")
            {
                if (__instance.transform.parent != null && __instance.transform.parent.parent.name != "enemyHealthHolder")
                {
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
    }
}
