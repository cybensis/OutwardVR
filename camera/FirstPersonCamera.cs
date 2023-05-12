using System;
using HarmonyLib;
using OutwardVR.body;
using OutwardVR.combat;
using UnityEngine;
using Valve.VR;

namespace OutwardVR.camera
{
    public class FirstPersonCamera
    {
        private static bool cameraFixed = true;
        public static bool enemyTargetActive = false;
        public const float NEAR_CLIP_PLANE_VALUE = 0.1f;
        private const float HMD_AND_BODY_DIFF_TOLERANCE = 17.5f;
        private static GameObject playerHead;
        private static GameObject playerTorso;
        public static GameObject leftHand;
        public static GameObject rightHand;

        public static bool freezeMovement = false;
        public static float freezeStartTime = 0f;

        public static float camInitYHeight = 0;
        public static float camCurrentHeight = 0;
        public static Transform camTransform;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainScreen), nameof(MainScreen.StartInit))]
        private static void OnCameraRigEnabled()
        {
            Camera.main.gameObject.AddComponent<SteamVR_TrackedObject>();
        }



        [HarmonyPatch(typeof(NetworkLevelLoader), "MidLoadLevel")]
        public class NetworkLevelLoader_MidLoadLevel
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                cameraFixed = false;
            }
        }




        [HarmonyPatch(typeof(CharacterCamera), "LateUpdate")]
        public class CharacterCamera_LateUpdate
        {
            [HarmonyPostfix]
            public static void Postfix(CharacterCamera __instance)
            {
                //-0.1f, 0.05f, 0.2f
                __instance.transform.localPosition = new Vector3(0,0.2f,0);
            }
        }

        private static void TriggerButton(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            camInitYHeight = Camera.main.transform.localPosition.y;
        }


        [HarmonyPatch(typeof(CharacterCamera), "Update")]
        public class CharacterCamera_Update
        {
            [HarmonyPrefix]
            public static bool Prefix(CharacterCamera __instance, Camera ___m_camera)
            {
                if (NetworkLevelLoader.Instance.IsOverallLoadingDone && UI.MenuPatches.isLoading)
                {
                    UI.MenuPatches.PositionMenuAfterLoading();
                    UI.MenuPatches.isLoading = false;
                }

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
                    CameraManager.Setup();
                    FixCamera(__instance, ___m_camera);
                    UI.MenuPatches.gameHasBeenLoadedOnce = true;
                    // CharacterUI is disabled during prologue so re-enable it here
                    __instance.TargetCharacter.CharacterUI.gameObject.active = true;
                    // Disable the loading cam once the player is loaded in
                    UI.MenuPatches.loadingCamHolder.gameObject.active = false;
                    // disable the head
                    __instance.TargetCharacter.Visuals.Head.GetComponent<SkinnedMeshRenderer>().enabled = false;
                    if (__instance.TargetCharacter.Visuals.DefaultHairVisuals != null)
                        __instance.TargetCharacter.Visuals.DefaultHairVisuals.GetComponent<SkinnedMeshRenderer>().enabled = false;


                    if (leftHand != null && leftHand.GetComponent<ArmIK>() == null)
                        leftHand.AddComponent<ArmIK>();
                    if (rightHand != null && rightHand.GetComponent<ArmIK>() == null)
                        rightHand.AddComponent<ArmIK>();
                    if (playerHead != null && playerHead.GetComponent<FixHeadRotation>() == null)
                        playerHead.AddComponent<FixHeadRotation>();
                    if (__instance.TargetCharacter.CurrentWeapon != null)
                    {
                        if (__instance.TargetCharacter.CurrentWeapon.Type == Weapon.WeaponType.FistW_2H)
                            __instance.TargetCharacter.CurrentWeapon.EquippedVisuals.gameObject.AddComponent<VRFisticuffsHandler>();
                        else
                            __instance.TargetCharacter.CurrentWeapon.EquippedVisuals.gameObject.AddComponent<VRMeleeHandler>();
                    }
                    if (__instance.TargetCharacter.LeftHandWeapon != null)
                    {
                        if (__instance.TargetCharacter.LeftHandWeapon.Type == Weapon.WeaponType.Shield)
                            __instance.TargetCharacter.LeftHandWeapon.EquippedVisuals.gameObject.AddComponent<VRShieldHandler>();
                        else if (__instance.TargetCharacter.LeftHandWeapon.Type == Weapon.WeaponType.Dagger_OH)
                            __instance.TargetCharacter.LeftHandWeapon.EquippedVisuals.gameObject.AddComponent<VRMeleeHandler>();
                    }

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
            SteamVR_Actions._default.Start.AddOnStateDownListener(TriggerButton, SteamVR_Input_Sources.Any);
            Controllers.Init();
            Camera.main.cullingMask = -1; // Culling mask needs to be -1, otherwise worldspace HUD doesn't show up
            Camera.main.nearClipPlane = NEAR_CLIP_PLANE_VALUE; // Reduce near clipping plane so the HUD can be seen when its close to the camera
            Canvas UICanvas = cameraScript.TargetCharacter.CharacterUI.UIPanel.gameObject.GetComponent<Canvas>();
            Camera.main.gameObject.AddComponent<SteamVR_TrackedObject>();
            cameraScript.TargetCharacter.CharacterUI.transform.parent.localRotation = Quaternion.identity;
            UICanvas.renderMode = RenderMode.WorldSpace;
            // set camera position and cancel out actual camera position
            var camHolder = camera.transform.parent;
            // get the root gameobject of the camera (parent of camHolder)
            var camRoot = camera.transform.root;
            // set the parent to the head transform, then reset local position
            if (playerHead)
                camRoot.SetParent(playerHead.transform, false);
            camRoot.ResetLocal();
            camHolder.localPosition = Vector3.zero;
            camRoot.rotation = Quaternion.identity;
            camRoot.localRotation = Quaternion.identity;
            cameraFixed = true;

            if (UICanvas)
            {
                cameraScript.TargetCharacter.CharacterUI.transform.parent.localPosition = Vector3.zero; // Set localPosition to zero to position HUD correctly
                cameraScript.TargetCharacter.CharacterUI.transform.parent.localScale = new Vector3(1f, 1f, 1f); // localScale is like 111.11 for some reason, so set it all to 1
                cameraScript.TargetCharacter.CharacterUI.transform.parent.parent.localScale = new Vector3(0.0006f, 0.0006f, 0.0006f); // The MenuManager is huge in world space as 1,1,1 so reduce it to 0.0006
            }
            Transform GeneralMenus = cameraScript.TargetCharacter.CharacterUI.transform.parent.parent.GetChild(2);
            if (GeneralMenus.name == "GeneralMenus")
            {
                GeneralMenus.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                //GeneralMenus.transform.localPosition = Vector3.zero;
                GeneralMenus.transform.position = UICanvas.transform.position;
                GeneralMenus.transform.rotation = Quaternion.identity;
                GeneralMenus.transform.localScale = new Vector3(1, 1, 1);
            }

            Debug.Log("[InwardVR] done setting up camera.");
        }


        [HarmonyPatch(typeof(CharacterJointManager), "Start")]
        public class SetHeadJoint
        {
            private static void Prefix(CharacterJointManager __instance)
            {
                if (__instance.name == "head" && __instance.transform.root.name != "AISquadManagerStructure")
                {
                    playerHead = __instance.transform.gameObject;
                }
                if (__instance.name == "hand_left" && __instance.transform.root.name != "AISquadManagerStructure")
                    leftHand = __instance.transform.gameObject;
                if (__instance.name == "hand_right" && __instance.transform.root.name != "AISquadManagerStructure")
                    rightHand = __instance.transform.gameObject;
            }
        }


        public static void SetFreezeMovement() { 
            freezeMovement = true;
            freezeStartTime = Time.time;
        }

        public static void UnfreezeMovement() {
            freezeMovement = false;
        }


        [HarmonyPatch(typeof(LocalCharacterControl), "UpdateMovement")]
        public class LocalCharacterControl_UpdateMovement
        {
            [HarmonyPrefix]
            public static bool Prefix(LocalCharacterControl __instance, ref Vector3 ___m_inputMoveVector, ref Vector3 ___m_modifMoveInput,
                Transform ___m_horiControl, ref bool ___m_sprintFacing)
            {
                Controllers.Update();
                // Should only need the boolean but use the timer just in case something goes wrong and the boolean doesn't get flipped, so it unfreezes after 2 seconds
                if (freezeMovement && Time.time - freezeStartTime >= 2) {
                    freezeMovement = false;
                }

                if (camInitYHeight == 0) {
                    camInitYHeight = Camera.main.transform.localPosition.y;
                    camTransform = Camera.main.transform;
                    __instance.Character.RagdollRoot.gameObject.AddComponent<VRCrouch>();
                }
                camCurrentHeight = Camera.main.transform.localPosition.y;

                Character m_char = __instance.m_character;
                Animator animator = m_char.Animator;
                TargetingSystem targetSys = m_char.TargetingSystem;

                CharacterController m_charControl = __instance.m_characterController;
                WindZone windZone = __instance.m_windZone;

                int turnAllow = __instance.m_turnAllowedInAction;
                float slopeSpeed = __instance.m_slopeSlowSpeed;

                enemyTargetActive = targetSys.Locked;

                // If the player isn't moving around, then make the camera rotate on the spot
                if (___m_modifMoveInput.y >= 0f && ___m_modifMoveInput.x != 0 && SteamVR_Actions._default.LeftJoystick.GetAxis(SteamVR_Input_Sources.Any).x == 0 && SteamVR_Actions._default.LeftJoystick.GetAxis(SteamVR_Input_Sources.Any).y == 0 && !targetSys.Locked)
                {
                    // If the player is moving ___m_modifMoveInput.y will be greater than 0.25f which makes turning super slow, but for first person
                    // the turning rate should be the same when moving as it is when you're still. Sprinting for some reason speeds up turning rate so use
                    // this value to make it slower
                    //if (m_char.Sprinting)
                    //    ___m_modifMoveInput.y = 0.5f;
                    //else
                    //    ___m_modifMoveInput.y = 0.25f;
                    ___m_modifMoveInput.y = 0.25f;

                    float yAmount = ___m_modifMoveInput.y;
                    if (yAmount < 0) yAmount *= -1;

                    // typical Y input will be 0 to 3.2
                    var yRatio = (float)((decimal)yAmount / (decimal)3.2f);
                    // Raise the second Lerp value to lower turn sensitivity
                    float hMod = Mathf.Lerp(0.01f, 0.04f, yRatio);
                    ___m_modifMoveInput.x *= hMod;
                }


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

                // ========= Custom code to move the in game body towards the headset when physically moving around =========
                // This gets the difference between the player body and the camera
                Vector3 camDistanceFromBody = __instance.transform.InverseTransformPoint(Camera.main.transform.position);
                // If the users x and y hmd axis is further away than 0.5 something must be wrong, so limit it to -+0.5 and -+2 for Y 
                camDistanceFromBody.x = Mathf.Clamp(camDistanceFromBody.x, -0.5f, 0.5f);
                camDistanceFromBody.y = Mathf.Clamp(camDistanceFromBody.y, -2f, 2f);
                camDistanceFromBody.z = Mathf.Clamp(camDistanceFromBody.z, -0.5f, 0.5f);

                // When sneaking, the player models head moves to the right, so I move the camera to the right to fix this which creates an offset
                // of 0.1 for the X axis, so use this to negate that
                if (__instance.Character.Sneaking)
                    camDistanceFromBody.x -= 0.1f;
                // Camera is positioned slightly forward from the bodies center, so use this to offset that
                camDistanceFromBody.z -= 0.2f;

                // If the camera is beyond -+0.1 distance from the body, then move it in that direction
                if (camDistanceFromBody.x >= 0.1f || camDistanceFromBody.x <= -0.1f)
                {
                    ___m_inputMoveVector.x += camDistanceFromBody.x * 2f;
                    Vector3 right = __instance.transform.right;
                    right.y = 0f;
                    // Since the cam holder is a child of the player body, we need to offset the movement with this
                    Camera.main.transform.parent.position += right * (camDistanceFromBody.x * -0.1f);
                }
                if (camDistanceFromBody.z > 0.1f || camDistanceFromBody.z <= -0.1f)
                {
                    ___m_inputMoveVector.y += camDistanceFromBody.z * 2f;
                    Vector3 forward = __instance.transform.forward;
                    forward.y = 0f;
                    Camera.main.transform.parent.position += forward * (camDistanceFromBody.z * -0.1f);
                }


                // ========= Custom code to lock Y axis =========
                // This is used to negate the headsets height and lock its Y axis
                Vector3 camPosition = Camera.main.transform.parent.localPosition;

                if (__instance.Character.Sneaking)
                    camPosition.y = Camera.main.transform.localPosition.y * -1f;
                else
                    camPosition.y = camInitYHeight * -1;
                Camera.main.transform.parent.localPosition = camPosition;

                // ========= Mix of custom and default code to enable sideways and backwards movement =========
                // This allows the player to move side to side only if a menu isn't open and they're not in dialogue

                if (freezeMovement)
                {
                    ___m_inputMoveVector.x = 0;
                    ___m_inputMoveVector.y = 0;
                }
                else { 
                    if (!m_char.CharacterUI.IsMenuFocused && !m_char.CharacterUI.IsDialogueInProgress)
                        ___m_inputMoveVector.x += SteamVR_Actions._default.LeftJoystick.GetAxis(SteamVR_Input_Sources.Any).x / 2;

                    // Moving backwards is pretty slow so increase it manually to speed it up. Set the if conditional to the inputMoveVector because the SteamVR input can be active during menus,
                    // whereas inputMoveVector cant
                    if (___m_inputMoveVector.y < -1)
                        ___m_inputMoveVector.y = SteamVR_Actions._default.LeftJoystick.GetAxis(SteamVR_Input_Sources.Any).y * 5f;

                    // Keep the movement from exceeding 10
                    ___m_inputMoveVector.y = Mathf.Clamp(___m_inputMoveVector.y, -10, 10);
                    ___m_inputMoveVector.x = Mathf.Clamp(___m_inputMoveVector.x, -10, 10);
                }

                var transformMove = ___m_horiControl.forward * ___m_inputMoveVector.y + ___m_horiControl.right * ___m_inputMoveVector.x;
                if (m_charControl.enabled)
                    m_charControl.Move(new Vector3(0f, -3f, 0f) * Time.deltaTime);


                Vector3 inverseMove = __instance.transform.InverseTransformDirection(transformMove) * slopeSpeed;

                var movementVector = inverseMove;
                // Between 0.5 to 2, and -2 to -0.5 are sweet spots where the horizontal movements gets really janky and slow, so increase movement speed manually here
                if ((inverseMove.x > 0.5 && inverseMove.x < 2) || (inverseMove.x < -0.5 && inverseMove.x > -2))
                    inverseMove.x *= 2f;
                if (m_char.AnimatorInitialized)
                {
                    animator.SetFloat("moveSide", inverseMove.x);
                    animator.SetFloat("moveForward", inverseMove.z);
                }

                if (m_char.Sliding || m_char.Falling)
                    m_charControl.Move(transformMove * Time.deltaTime);

                if (m_char.UseLegacyVisual && windZone != null)
                {
                    float b = inverseMove.magnitude * 0.5f + (m_char.NextIsLocomotion ? 0 : 3);
                    windZone.windTurbulence = Mathf.Lerp(windZone.windTurbulence, b, 2f * Time.deltaTime);
                }
                // ========= Default code, I think for rotating the body =========
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
                            // This code makes moving left and right rotate the camera instead, so keep it commented out
                            //if (___m_modifMoveInput.magnitude > 0.2f)
                            //    inputOne = new Vector2(transformMove.x, transformMove.z);
                        }
                        else if (!targetSys.Locked || m_char.CharacterCamera.InDeployBuildingMode)
                            inputOne = new Vector2(___m_horiControl.forward.x, ___m_horiControl.forward.z);
                        else
                        {
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
                    if (!targetSys.Locked)
                    {
                        // Rotate body to match camera position - Needs some work
                        Vector3 vrRot = Camera.main.transform.rotation.eulerAngles;
                        Vector3 bodyRot = __instance.transform.rotation.eulerAngles;
                        if (Mathf.DeltaAngle(vrRot.y, bodyRot.y) > HMD_AND_BODY_DIFF_TOLERANCE) // If there is a difference of 17.5f between the body and camera rotation
                        {
                            // for every 17.5 degrees of difference in body and camera rotation, rotate the player x * -2f to rotate left or x * 2f to rotate right
                            clampedDiff = -2f * (Mathf.DeltaAngle(vrRot.y, bodyRot.y) / HMD_AND_BODY_DIFF_TOLERANCE);
                            // rotate camera's parents parent the same amount in the reverse direction to offset the rotation of its parent
                            Camera.main.transform.parent.parent.Rotate(0f, clampedDiff * -1, 0f, Space.World);
                        }
                        else if (Mathf.DeltaAngle(vrRot.y, bodyRot.y) < -HMD_AND_BODY_DIFF_TOLERANCE)
                        {
                            clampedDiff = 2f * (Mathf.DeltaAngle(vrRot.y, bodyRot.y) / -HMD_AND_BODY_DIFF_TOLERANCE);
                            Camera.main.transform.parent.parent.Rotate(0f, clampedDiff * -1, 0f, Space.World);
                        }
                    }
                    clampedDiff += SteamVR_Actions._default.RightJoystick.axis.x * 4f;
                    __instance.transform.Rotate(0f, clampedDiff, 0f);
                }

                if (Global.CheatsEnabled)
                    m_char.NoFall = __instance.MovementMultiplier > 4f;

                __instance.m_localMovementVector = movementVector;

                return false;
            }
        }
    }
}
