using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Valve.VR;

namespace OutwardVR
{
    public class FirstPersonCamera
    {
        private static bool cameraFixed = true;

        [HarmonyPatch(typeof(NetworkLevelLoader), "MidLoadLevel")]
        public class NetworkLevelLoader_MidLoadLevel
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                cameraFixed = false;
            }
        }

        // Disable mouse/controller look rotation

        //[HarmonyPatch(typeof(CharacterCamera), "LateUpdate")]
        //public class CharacterCamera_LateUpdate
        //{
        //    [HarmonyPrefix]
        //    public static bool Prefix()
        //    {
        //        return false;
        //    }
        //}

        // Override Camera update

        [HarmonyPatch(typeof(CharacterCamera), "Update")]
        public class CharacterCamera_Update
        {
            [HarmonyPrefix]
            public static bool Prefix(CharacterCamera __instance, Camera ___m_camera)
            {

                if (cameraFixed
                    || !__instance.TargetCharacter
                    || !NetworkLevelLoader.Instance.IsOverallLoadingDone
                    || !NetworkLevelLoader.Instance.AllPlayerDoneLoading
                    || !NetworkLevelLoader.Instance.AllPlayerReadyToContinue
                    || MenuManager.Instance.IsReturningToMainMenu)
                {
                    return false;
                }

                try
                {
                    FixCamera(__instance, ___m_camera);
                    __instance.transform.parent.gameObject.GetComponent<SkinnedMeshRenderer>().enabled = false;
                    __instance.transform.parent.transform.parent.transform.GetChild(8).GetComponent<SkinnedMeshRenderer>().enabled = false;

                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }

                return false;
            }
        }

        private static void FixCamera(CharacterCamera cameraScript, Camera camera)
        {
            Debug.Log("[InwardVR] setting up camera...");
            //Notes: TargetCharacter links to the Character class
            Controllers.Init();

            // Get the character model head transform
            var headTrans = cameraScript.TargetCharacter.Visuals.Head.transform;

            // set camera position and cancel out actual camera position
            var camHolder = camera.transform.parent;
            camHolder.localPosition = camera.transform.localPosition * -1;
            var pos = camHolder.localPosition;

            /*pos.x -= 0.05f;*/
            pos.y += 0.7f;
            /* pos.z -= 0.05f;*/
            camHolder.localPosition = pos;

            // get the root gameobject of the camera (parent of camHolder)
            var camRoot = camera.transform.root;
            // set the parent to the head transform, then reset local position
            camRoot.SetParent(headTrans, false);
            camRoot.ResetLocal();

            // align rotation with the character rotation
            camRoot.rotation = cameraScript.TargetCharacter.transform.rotation;

            cameraFixed = true;
            cameraScript.transform.Rotate(351f, 250f, 346f);
            //if (CameraManager.RightHand == null)
            //{
            //    if (AssetLoader.LeftHandBase == null)
            //    {
            //        new AssetLoader();
            //    }
            //    CameraManager.VROrigin = new GameObject();
            //    Transform OriginalCameraParent = Camera.main.transform.parent;
            //    CameraManager.VROrigin.transform.position = OriginalCameraParent.position;
            //    CameraManager.VROrigin.transform.rotation = OriginalCameraParent.rotation;
            //    CameraManager.VROrigin.transform.localScale = OriginalCameraParent.localScale;

            //    CameraManager.VROrigin.transform.parent = OriginalCameraParent;

            //        CameraManager.RightHand = GameObject.Instantiate(AssetLoader.RightHandBase, Vector3.zeroVector, Quaternion.identityQuaternion);
            //    CameraManager.RightHand.transform.parent = OriginalCameraParent;
            //        CameraManager.LeftHand = GameObject.Instantiate(AssetLoader.LeftHandBase, Vector3.zeroVector, Quaternion.identityQuaternion);
            //    CameraManager.LeftHand.transform.parent = OriginalCameraParent;

            //}
            Debug.Log("[InwardVR] done setting up camera.");
        }

        // Make CharacterControl less sharp turning

        private static readonly BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        private static readonly FieldInfo fi_m_character = typeof(CharacterControl).GetField("m_character", flags);
        private static readonly FieldInfo fi_m_controller = typeof(CharacterControl).GetField("m_characterController", flags);
        private static readonly FieldInfo fi_m_windZone = typeof(CharacterControl).GetField("m_windZone", flags);
        private static readonly FieldInfo fi_localMoveVector = typeof(CharacterControl).GetField("m_localMovementVector", flags);
        private static readonly FieldInfo fi_turnAllow = typeof(CharacterControl).GetField("m_turnAllowedInAction", flags);
        private static readonly FieldInfo fi_slopeSpeed = typeof(CharacterControl).GetField("m_slopeSlowSpeed", flags);

        private static readonly Dictionary<UID, float> LastTurnTimes = new Dictionary<UID, float>();


        [HarmonyPatch(typeof(LocalCharacterControl), "UpdateMovement")]
        public class LocalCharacterControl_UpdateMovement
        {
            [HarmonyPrefix]
            public static bool Prefix(LocalCharacterControl __instance, ref Vector3 ___m_inputMoveVector, ref Vector3 ___m_modifMoveInput,
                Transform ___m_horiControl, ref bool ___m_sprintFacing)
            {
                Controllers.Update();
                var m_char = fi_m_character.GetValue(__instance) as Character;
                var animator = m_char.Animator;
                var targetSys = m_char.TargetingSystem;

                var m_charControl = fi_m_controller.GetValue(__instance) as CharacterController;
                var windZone = fi_m_windZone.GetValue(__instance) as WindZone;

                var turnAllow = (int)fi_turnAllow.GetValue(__instance);
                var slopeSpeed = (float)fi_slopeSpeed.GetValue(__instance);

                // ~~~~~~~~~~~ custom turn speed override ~~~~~~~~~~~

                // turn around 
                if (___m_modifMoveInput.y < 0f && !targetSys.Locked)
                {
                    if (!LastTurnTimes.ContainsKey(m_char.UID))
                    {
                        LastTurnTimes.Add(m_char.UID, float.MinValue);
                    }
                    if (Time.time - LastTurnTimes[m_char.UID] > 1f)
                    {
                        LastTurnTimes[m_char.UID] = Time.time;

                        var rot = m_char.transform.localEulerAngles;
                        rot.y += 180f;
                        m_char.transform.localEulerAngles = rot;
                    }
                    ___m_modifMoveInput.y = 0f;
                }
                // turn speed override
                else if (___m_modifMoveInput.x != 0 && !targetSys.Locked)
                {
                    Logs.WriteInfo("test");
                    Logs.WriteInfo(___m_modifMoveInput.x);
                    Logs.WriteInfo(___m_modifMoveInput.y);
                    if (___m_modifMoveInput.y < 0.25f)
                    {
                        ___m_modifMoveInput.y = 0.25f;
                    }

                    float yAmount = ___m_modifMoveInput.y;
                    if (yAmount < 0) yAmount *= -1;

                    // typical Y input will be 0 to 3.2
                    var yRatio = (float)((decimal)yAmount / (decimal)3.2f);
                    float hMod = Mathf.Lerp(0.01f, 0.05f, yRatio);

                    ___m_modifMoveInput.x *= hMod;
                }

                // ~~~~~~~~~~~~~~~~~~~ end custom ~~~~~~~~~~~~~~~~~~~

                // ========= all vanilla =========
                float moveModif = 4f;

                if (Vector3.Angle(___m_inputMoveVector, ___m_modifMoveInput) > 160f)
                {
                    ___m_modifMoveInput += Vector3.one * 0.0005f;
                    moveModif = 8f;
                }

                ___m_inputMoveVector = Vector3.RotateTowards(
                    ___m_inputMoveVector,
                    ___m_modifMoveInput,
                    Vector3.Angle(___m_inputMoveVector, ___m_modifMoveInput) * 0.1f * Time.deltaTime,
                    0f);

                ___m_inputMoveVector = Vector2.MoveTowards(
                    ___m_inputMoveVector,
                    ___m_modifMoveInput,
                    Vector2.Distance(___m_inputMoveVector, ___m_modifMoveInput) * moveModif * Time.deltaTime);

                var transformMove = (___m_horiControl.forward * ___m_inputMoveVector.y) + (___m_horiControl.right * ___m_inputMoveVector.x);

                if (m_charControl.enabled)
                {
                    m_charControl.Move(new Vector3(0f, -3f, 0f) * Time.deltaTime);
                }

                Vector3 inverseMove = __instance.transform.InverseTransformDirection(transformMove) * slopeSpeed;
                inverseMove.x *= 0.8f;
                if (inverseMove.z < 0f)
                {
                    inverseMove.z *= 0.6f;
                }
                var movementVector = inverseMove;

                if (m_char.AnimatorInitialized)
                {
                    animator.SetFloat("moveSide", inverseMove.x);
                    animator.SetFloat("moveForward", inverseMove.z);
                }

                if (m_char.Sliding || m_char.Falling)
                {
                    m_charControl.Move(transformMove * Time.deltaTime);
                }

                if (m_char.UseLegacyVisual && windZone != null)
                {
                    float b = inverseMove.magnitude * 0.5f + (float)(m_char.NextIsLocomotion ? 0 : 3);
                    windZone.windTurbulence = Mathf.Lerp(windZone.windTurbulence, b, 2f * Time.deltaTime);
                }

                if (!m_char.Sliding)
                {
                    Vector2 inputOne;
                    Vector2 inputTwo = inputOne = new Vector2(__instance.transform.forward.x, __instance.transform.forward.z);

                    if (m_char.LocomotionAllowed
                        && (turnAllow > 0
                            || m_char.InLocomotion
                            || m_char.NextIsLocomotion
                            || m_char.Sliding))
                    {
                        if (!__instance.FaceLikeCamera || m_char.Dodging || ___m_sprintFacing)
                        {
                            if (___m_modifMoveInput.magnitude > 0.2f)
                            {
                                inputOne = new Vector2(transformMove.x, transformMove.z);
                            }
                        }
                        else if (!targetSys.Locked || m_char.CharacterCamera.InDeployBuildingMode)
                        {
                            inputOne = new Vector2(___m_horiControl.forward.x, ___m_horiControl.forward.z);
                        }
                        else
                        {
                            var targetDiff = targetSys.AdjustedLockedPointPos - __instance.transform.position;
                            inputOne = new Vector2(targetDiff.x, targetDiff.z).normalized;
                        }
                    }

                    float angleDiff = Vector2.Angle(inputTwo, inputOne);

                    if (Vector3.Cross(inputTwo, inputOne).z > 0f)
                    {
                        angleDiff = 0f - angleDiff;
                    }

                    float clampedDiff = Mathf.Clamp(angleDiff, -10f, 10f) * 50f * Time.deltaTime;
                    if (!(angleDiff > 0f))
                    {
                        clampedDiff = Mathf.Clamp(clampedDiff, angleDiff, 0f - angleDiff);
                    }
                    else
                    {
                        clampedDiff = Mathf.Clamp(clampedDiff, 0f - angleDiff, angleDiff);
                    }

                    __instance.transform.Rotate(0f, clampedDiff, 0f);
                }

                if (Global.CheatsEnabled)
                {
                    m_char.NoFall = (__instance.MovementMultiplier > 4f);
                }

                // set values that we used manual reflection for
                /* fi_localMoveVector.SetValue(__instance, movementVector);
                 fi_turnAllow.SetValue(__instance, turnAllow);
                 fi_slopeSpeed.SetValue(__instance, slopeSpeed);*/

                return false;
            }
        }
    }
}
