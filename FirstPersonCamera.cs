using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using Valve.VR;
using static MapMagic.ObjectPool;

namespace OutwardVR
{
    public class FirstPersonCamera
    {
        private static bool cameraFixed = true;
        public static bool enemyTargetActive = false;

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
                    __instance.transform.parent.gameObject.GetComponent<SkinnedMeshRenderer>().enabled = false; // disable the head
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
            Controllers.Init();
            Camera.main.cullingMask = -1; // Culling mask needs to be -1, otherwise worldspace HUD doesn't show up
            Camera.main.nearClipPlane = 0.001f; // Reduce near clipping plane so the HUD can be seen when its close to the camera
            Canvas UICanvas = cameraScript.TargetCharacter.CharacterUI.UIPanel.gameObject.GetComponent<Canvas>();

            cameraScript.TargetCharacter.CharacterUI.transform.parent.localRotation = Quaternion.identity; 


            UICanvas.renderMode = RenderMode.WorldSpace;
            var headTrans = cameraScript.TargetCharacter.Visuals.Head.transform; // Get the character model head transform

            // set camera position and cancel out actual camera position
            var camHolder = camera.transform.parent; 
            camHolder.localPosition = camera.transform.localPosition * -1;
            var pos = camHolder.localPosition;
            pos.y += 0.7f; // This offset places the camera at the right height
            camHolder.localPosition = pos + (camHolder.forward * 0.115f) + (camHolder.right * 0.09f);

            // get the root gameobject of the camera (parent of camHolder)
            var camRoot = camera.transform.root;
            // set the parent to the head transform, then reset local position
            camRoot.SetParent(headTrans, false);
            camRoot.ResetLocal();

            // align rotation with the character rotation
            camRoot.rotation = cameraScript.TargetCharacter.transform.rotation;

            cameraFixed = true;
            //cameraScript.transform.Rotate(351f, 250f, 346f);
            cameraScript.transform.Rotate(348.42f, 250f, 341.36f);

            if (UICanvas)
            {
                cameraScript.TargetCharacter.CharacterUI.transform.parent.localPosition = Vector3.zero; // Set localPosition to zero to position HUD correctly
                cameraScript.TargetCharacter.CharacterUI.transform.parent.localScale = new Vector3(1f,1f,1f); // localScale is like 111.11 for some reason, so set it all to 1
                UICanvas.transform.root.localScale = new Vector3(0.0006f, 0.0006f, 0.0006f); // The MenuManager is huge in world space as 1,1,1 so reduce it to 0.0006

            }
            Transform GeneralMenus = UICanvas.transform.root.GetChild(2); // Maybe change this to loop over all children, its place might change
            if (GeneralMenus.name == "GeneralMenus") { 
                GeneralMenus.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                //GeneralMenus.transform.localPosition = Vector3.zero;
                GeneralMenus.transform.position = UICanvas.transform.position;
                GeneralMenus.transform.rotation = Quaternion.identity;
                GeneralMenus.transform.localScale = new Vector3(1, 1, 1);
            } 

            Debug.Log("[InwardVR] done setting up camera.");
        }

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
            private static bool startedSneaking = false;
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

                enemyTargetActive = targetSys.Locked;

                // ~~~~~~~~~~~ custom turn speed override ~~~~~~~~~~~

                // turn around 
                if (___m_modifMoveInput.y < 0f && !targetSys.Locked)
                {
                    //if (!LastTurnTimes.ContainsKey(m_char.UID))
                    //{
                    //    LastTurnTimes.Add(m_char.UID, float.MinValue);
                    //}
                    //if (Time.time - LastTurnTimes[m_char.UID] > 1f)
                    //{
                    //    LastTurnTimes[m_char.UID] = Time.time;

                    //    var rot = m_char.transform.localEulerAngles;
                    //    rot.y += 180f;
                    //    m_char.transform.localEulerAngles = rot;
                    //}
                    //___m_modifMoveInput.y = 0f;
                }
                //turn speed
                else if (___m_modifMoveInput.x != 0 && !targetSys.Locked)
                {

                    // If the player is moving ___m_modifMoveInput.y will be greater than 0.25f which makes turning super slow, but for first person
                    // the turning rate should be the same when moving as it is when you're still. Sprinting for some reason speeds up turning rate so use
                    // this value to make it slower
                    if (m_char.Sprinting)
                        ___m_modifMoveInput.y = 0.5f;
                    else
                        ___m_modifMoveInput.y = 0.25f;

                    float yAmount = ___m_modifMoveInput.y;
                    if (yAmount < 0) yAmount *= -1;                   

                    // typical Y input will be 0 to 3.2
                    var yRatio = (float)((decimal)yAmount / (decimal)3.2f);
                    //float hMod = Mathf.Lerp(0.01f, 0.05f, yRatio);
                    float hMod = Mathf.Lerp(0.01f, 0.015f, yRatio);
                    ___m_modifMoveInput.x *= hMod;
                }

                // ~~~~~~~~~~~~~~~~~~~ end custom ~~~~~~~~~~~~~~~~~~~

                // ========= vanilla =========
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

                // ========= More custom =========
                Vector3 camDistanceFromBody = __instance.transform.InverseTransformPoint(Camera.main.transform.position);
                // When sneaking, the player models head moves to the right, so I move the camera to the right to fix this which creates an offset
                // of 0.1 for the X axis, so use this to negate that
                if (__instance.Character.Sneaking)
                    camDistanceFromBody.x -= 0.1f;
                // Camera is positioned slightly forward from the bodies center, so use this to offset that
                camDistanceFromBody.z -= 0.2f;

                if (camDistanceFromBody.x >= 0.1f || camDistanceFromBody.x <= -0.1f) {
                    ___m_inputMoveVector.x += camDistanceFromBody.x * 2f;
                    Vector3 right = __instance.transform.right;
                    right.y = 0f;
                    Camera.main.transform.parent.position += (right * (camDistanceFromBody.x * -0.1f));
                }
                if (camDistanceFromBody.z > 0.1f || camDistanceFromBody.z <= -0.1f) {
                    ___m_inputMoveVector.y += camDistanceFromBody.z * 2f;
                    Vector3 forward = __instance.transform.forward;
                    forward.y = 0f;
                    Camera.main.transform.parent.position += (forward * (camDistanceFromBody.z * -0.1f));
                }

                // This if/else statement locks the characters Y axis to the perfect position, then also when the player crouches or uncrouches, it changes the position
                // a little bit since the crouching head is more to the right and forward
                Vector3 camPosition = Camera.main.transform.parent.localPosition;
                camPosition.y = Camera.main.transform.localPosition.y * -1f;
                if (__instance.Character.Sneaking) {
                    camPosition.y += 0.225f;
                    // Don't want the X axis to be locked in so only set the X axis crouching offset the one time
                    if (startedSneaking == false) {
                        startedSneaking = true;
                        // Negative in this instance moves it forward and right respectively, not backwards and left
                        camPosition += __instance.transform.right * -0.25f;
                        camPosition += __instance.transform.forward * -0.45f;
                    }
                }
                else {
                    camPosition.y += 0.65f;
                    // Return the players X axis position back to normal after returning from crouching
                    if (startedSneaking) { 
                        startedSneaking = false;
                        camPosition += __instance.transform.right * 0.25f;
                        camPosition += __instance.transform.forward * 0.45f;
                    }
                }
                Camera.main.transform.parent.localPosition = camPosition;
                // This allows the player to move side to side only if a menu isn't open and they're not moving forward because it makes the
                // camera turn around when you move forward and try to side step
                if (!m_char.CharacterUI.IsMenuFocused) {
                    if (SteamVR_Actions._default.LeftJoystick.GetAxis(SteamVR_Input_Sources.Any).x > 0 || SteamVR_Actions._default.LeftJoystick.GetAxis(SteamVR_Input_Sources.Any).x < 0)
                        ___m_inputMoveVector.x += SteamVR_Actions._default.LeftJoystick.GetAxis(SteamVR_Input_Sources.Any).x / 2;
                }

                // ========= End of custom =========

                // setting y to positive will move character forward, negative moves backward
                // setting x to postiive will move character right, negative will move left
                var transformMove = (___m_horiControl.forward * ___m_inputMoveVector.y) + (___m_horiControl.right * ___m_inputMoveVector.x);

                if (m_charControl.enabled)
                    m_charControl.Move(new Vector3(0f, -3f, 0f) * Time.deltaTime);

                Vector3 inverseMove = __instance.transform.InverseTransformDirection(transformMove) * slopeSpeed;
                inverseMove.x *= 0.8f;
                if (inverseMove.z < 0f)
                    inverseMove.z *= 0.6f;

                var movementVector = inverseMove;

                if (m_char.AnimatorInitialized) {
                    animator.SetFloat("moveSide", inverseMove.x);
                    animator.SetFloat("moveForward", inverseMove.z);
                }

                if (m_char.Sliding || m_char.Falling)
                    m_charControl.Move(transformMove * Time.deltaTime);

                if (m_char.UseLegacyVisual && windZone != null) {
                    float b = inverseMove.magnitude * 0.5f + (float)(m_char.NextIsLocomotion ? 0 : 3);
                    windZone.windTurbulence = Mathf.Lerp(windZone.windTurbulence, b, 2f * Time.deltaTime);
                }

                if (!m_char.Sliding) {
                    Vector2 inputOne;
                    Vector2 inputTwo = inputOne = new Vector2(__instance.transform.forward.x, __instance.transform.forward.z);

                    if (m_char.LocomotionAllowed
                        && (turnAllow > 0
                            || m_char.InLocomotion
                            || m_char.NextIsLocomotion
                            || m_char.Sliding))
                    {
                        if (!__instance.FaceLikeCamera || m_char.Dodging || ___m_sprintFacing) {
                            // This code makes moving left and right rotate the camera instead, so keep it commented out
                            //if (___m_modifMoveInput.magnitude > 0.2f)
                            //    inputOne = new Vector2(transformMove.x, transformMove.z);
                        }
                        else if (!targetSys.Locked || m_char.CharacterCamera.InDeployBuildingMode)
                            inputOne = new Vector2(___m_horiControl.forward.x, ___m_horiControl.forward.z);
                        else {
                            var targetDiff = targetSys.AdjustedLockedPointPos - __instance.transform.position;
                            inputOne = new Vector2(targetDiff.x, targetDiff.z).normalized;
                        }
                    }

                    float angleDiff = Vector2.Angle(inputTwo, inputOne);
                    if (Vector3.Cross(inputTwo, inputOne).z > 0f)
                        angleDiff = 0f - angleDiff;

                    float clampedDiff = Mathf.Clamp(angleDiff, -10f, 10f) * 50f * Time.deltaTime;
                    if (!(angleDiff > 0f))
                        clampedDiff = Mathf.Clamp(clampedDiff, angleDiff, 0f - angleDiff);
                    else
                        clampedDiff = Mathf.Clamp(clampedDiff, 0f - angleDiff, angleDiff);

                    // ========= custom code for rotating body when looking around =========
                    if (!targetSys.Locked) {
                        // Rotate body to match camera position - Needs some work
                        Vector3 vrRot = Camera.main.transform.rotation.eulerAngles;
                        Vector3 bodyRot = __instance.transform.rotation.eulerAngles;
                        if (Mathf.DeltaAngle(vrRot.y, bodyRot.y) > 25f) // If there is a difference of 10f between the body and camera rotation
                        {
                            // for every 10 degrees of difference in body and camera rotation, rotate the player x * -2f to rotate left or x * 2f to rotate right
                            clampedDiff = -2f * (Mathf.DeltaAngle(vrRot.y, bodyRot.y) / 25);
                            // rotate camera's parents parent the same amount in the reverse direction to offset the rotation of its parent
                            Camera.main.transform.parent.parent.Rotate(0f, clampedDiff * -1, 0f, Space.World);
                        }
                        else if (Mathf.DeltaAngle(vrRot.y, bodyRot.y) < -25f)
                        {
                            clampedDiff = 2f * (Mathf.DeltaAngle(vrRot.y, bodyRot.y) / -25);
                            Camera.main.transform.parent.parent.Rotate(0f, clampedDiff * -1, 0f, Space.World);
                        }

                    }
                    clampedDiff += SteamVR_Actions._default.RightJoystick.axis.x * 4f;
                    __instance.transform.Rotate(0f, clampedDiff, 0f);
                }

                if (Global.CheatsEnabled)
                {
                    m_char.NoFall = (__instance.MovementMultiplier > 4f);
                }

                // set values that we used manual reflection for
                fi_localMoveVector.SetValue(__instance, movementVector);
                fi_turnAllow.SetValue(__instance, turnAllow);
                fi_slopeSpeed.SetValue(__instance, slopeSpeed);

                return false;
            }
        }
    }
}
