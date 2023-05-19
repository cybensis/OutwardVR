using System;
using System.Diagnostics;
using HarmonyLib;
using NodeCanvas.Framework;
using OutwardVR;
using OutwardVR.body;
using OutwardVR.combat;
using UnityEngine;
using Valve.VR;
using static MapMagic.ObjectPool;

namespace OutwardVR.camera
{
    public class CameraHandler
    {
        public static bool cameraFixed = true;
        public static bool enemyTargetActive = false;
        public const float NEAR_CLIP_PLANE_VALUE = 0.1f;
        private const float HMD_AND_BODY_DIFF_TOLERANCE = 17.5f;

        public static bool freezeMovement = false;
        public static float freezeStartTime = 0f;

        public static float camInitYHeight = 0;
        public static float camCurrentHeight = 0;
        private static Vector3 nonBobHeadLocalPos = new Vector3(0.2f, 1.6f, 0);
        public static Transform camTransform;



        [HarmonyPatch(typeof(Character), "DodgeInput", new Type[] { typeof(Vector3) })]
        public class CorrectDodgeDireciton
        {
            [HarmonyPrefix]
            public static void Prefix(Character __instance, ref Vector3 _direction)
            {
                _direction = __instance.transform.forward * SteamVR_Actions._default.LeftJoystick.axis.y + __instance.transform.right * SteamVR_Actions._default.LeftJoystick.axis.x;
            }
        }


        [HarmonyPatch(typeof(Character), "DodgeEndAnimEvent")]
        public class FixCamAfterDodge
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                if (!VRInstanceManager.firstPerson)
                    VRInstanceManager.camRoot.transform.rotation = VRInstanceManager.characterInstance.transform.rotation;
            }
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

        private static bool hasBodyBeenDisabledOnSleep = false;


        [HarmonyPatch(typeof(CharacterCamera), "LateUpdate")]
        public class CharacterCamera_LateUpdate
        {
            [HarmonyPostfix]
            public static void Postfix(CharacterCamera __instance)
            {
                if (!NetworkLevelLoader.Instance.IsOverallLoadingDone || !NetworkLevelLoader.Instance.AllPlayerReadyToContinue)
                    return;
                if (VRInstanceManager.firstPerson)
                {
                    __instance.transform.localPosition = new Vector3(0, 0f, 0);
                    if (VRInstanceManager.headBobOn)
                        __instance.transform.localPosition = new Vector3(0, 0.15f, 0);
                    else if (!VRInstanceManager.headBobOn && __instance.m_targetCharacter.ReadyToSleep)
                        __instance.transform.parent.localPosition = new Vector3(0.2f, 0.6f, 0);
                    else
                    {
                        Vector3 newPos = nonBobHeadLocalPos;
                        newPos.y -= 0.29f * Mathf.Clamp(CameraHandler.camInitYHeight - CameraHandler.camCurrentHeight, 0, 1) * 1.25f;
                        if (__instance.m_targetCharacter.Sneaking)
                            newPos.y -= 0.3f;
                        __instance.transform.parent.localPosition = newPos;
                    }

                    if ((!hasBodyBeenDisabledOnSleep && __instance.m_targetCharacter.ReadyToSleep) || (hasBodyBeenDisabledOnSleep && !__instance.m_targetCharacter.ReadyToSleep))
                    {
                        CharacterVisuals visuals = __instance.m_targetCharacter.Visuals;
                        bool activeVisuals = !__instance.m_targetCharacter.ReadyToSleep;
                        if (visuals.EquippedBodyVisuals != null)
                            visuals.EquippedBodyVisuals.gameObject.SetActive(activeVisuals);
                        if (visuals.EquippedFootVisuals != null)
                            visuals.EquippedFootVisuals.gameObject.SetActive(activeVisuals);
                        if (visuals.EquippedBodyVisuals == null && visuals.DefaultBodyVisuals != null)
                            visuals.DefaultBodyVisuals.gameObject.SetActive(activeVisuals);
                        hasBodyBeenDisabledOnSleep = !hasBodyBeenDisabledOnSleep;

                    }
                }
                else
                {
                    Vector3 newPos = new Vector3(0, (camCurrentHeight * -1) + 1.2f, -1.2f);
                    Camera.main.transform.parent.localPosition = newPos;
                }
                //Camera.main.transform.parent.localPosition = new Vector3(0,0,-1) + (Camera.main.transform.localPosition * -1);
            }
        }

        private static void TriggerButton(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            camInitYHeight = Camera.main.transform.localPosition.y;
            if (!VRInstanceManager.firstPerson && VRInstanceManager.characterInstance != null) {
                VRInstanceManager.camRoot.transform.rotation = Quaternion.identity;
                VRInstanceManager.camRoot.transform.Rotate(0, Camera.main.transform.localEulerAngles.y,0);
            
            }
        }


        [HarmonyPatch(typeof(CharacterCamera), "Update")]
        public class CharacterCamera_Update
        {
            [HarmonyPrefix]
            public static bool Prefix(CharacterCamera __instance, Camera ___m_camera)
            {
                if (NetworkLevelLoader.Instance.IsOverallLoadingDone && VRInstanceManager.isLoading)
                {
                    UI.MenuPatches.PositionMenuAfterLoading();
                    VRInstanceManager.isLoading = false;
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
                    VRInstanceManager.isLoading = false;

                }
                catch (Exception e)
                {
                    Logs.WriteError(e.ToString());
                }
                return false;
            }
        }



        private static void FixCamera(CharacterCamera cameraScript, Camera camera)
        {
            Logs.WriteInfo("[InwardVR] setting up camera...");
            VRInstanceManager.characterInstance = cameraScript.TargetCharacter;
            VRInstanceManager.nonBobPlayerHead = cameraScript.TargetCharacter.Visuals.Head.gameObject;
            VRInstanceManager.gameHasBeenLoadedOnce = true;

            SteamVR_Actions._default.Start.AddOnStateDownListener(TriggerButton, SteamVR_Input_Sources.Any);
            Controllers.Init();
            // Culling mask needs to be -1, otherwise worldspace HUD doesn't show up
            Camera.main.cullingMask = -1; 
            // Reduce near clipping plane so the HUD can be seen when its close to the camera
            Camera.main.nearClipPlane = NEAR_CLIP_PLANE_VALUE; 
            Canvas UICanvas = cameraScript.TargetCharacter.CharacterUI.UIPanel.gameObject.GetComponent<Canvas>();
            Camera.main.gameObject.AddComponent<SteamVR_TrackedObject>();
            cameraScript.TargetCharacter.CharacterUI.transform.parent.localRotation = Quaternion.identity;
            UICanvas.renderMode = RenderMode.WorldSpace;
            // get the root gameobject of the camera (parent of camHolder)
            VRInstanceManager.camRoot = cameraScript.gameObject;
            // set the parent to the head transform, then reset local position
            if (VRInstanceManager.firstPerson)
                SetupFirstPerson(cameraScript, camera);
            else
                SetupThirdPerson(cameraScript, camera);

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

            // CharacterUI is disabled during prologue so re-enable it here
            cameraScript.TargetCharacter.CharacterUI.gameObject.active = true;
            // Disable the loading cam once the player is loaded in
            UI.MenuPatches.loadingCamHolder.gameObject.active = false;
            Logs.WriteInfo("[InwardVR] done setting up camera.");
        }

        private static void SetupThirdPerson(CharacterCamera cameraScript, Camera camera)
        {
            VRInstanceManager.camRoot.transform.parent = null;
            VRInstanceManager.camRoot.transform.rotation = VRInstanceManager.characterInstance.transform.rotation;
            cameraFixed = true;

        }

        private static void SetupFirstPerson(CharacterCamera cameraScript, Camera camera)
        {
            if (VRInstanceManager.headBobOn)
                VRInstanceManager.camRoot.transform.SetParent(VRInstanceManager.modelPlayerHead.transform, false);
            else
                VRInstanceManager.camRoot.transform.SetParent(VRInstanceManager.nonBobPlayerHead.transform, false);

            // set camera position and cancel out actual camera position
            Transform camHolder = camera.transform.parent;
            VRInstanceManager.camRoot.transform.ResetLocal();
            camHolder.localPosition = Vector3.zero;
            VRInstanceManager.camRoot.transform.rotation = Quaternion.identity;
            VRInstanceManager.camRoot.transform.localRotation = Quaternion.identity;
            cameraFixed = true;

            // disable the visuals that block first person view
            VRInstanceManager.nonBobPlayerHead.GetComponent<SkinnedMeshRenderer>().enabled = false;
            if (cameraScript.TargetCharacter.Visuals.DefaultHairVisuals != null)
            {
                VRInstanceManager.playerHair = cameraScript.TargetCharacter.Visuals.DefaultHairVisuals.GetComponent<SkinnedMeshRenderer>();
                VRInstanceManager.playerHair.enabled = false;
            }
            if (cameraScript.TargetCharacter.Visuals.ActiveVisualsHelmOrHead != null)
            {
                VRInstanceManager.activeVisualsHelmOrHead = cameraScript.TargetCharacter.Visuals.ActiveVisualsHelmOrHead.Renderer;
                VRInstanceManager.activeVisualsHelmOrHead.enabled = false;
            }
            // Add any first person components
            if (VRInstanceManager.modelLeftHand != null && VRInstanceManager.leftHandIK == null)
                VRInstanceManager.leftHandIK = VRInstanceManager.modelLeftHand.AddComponent<ArmIK>();
            if (VRInstanceManager.modelRightHand != null && VRInstanceManager.rightHandIK == null)
                VRInstanceManager.rightHandIK = VRInstanceManager.modelRightHand.AddComponent<ArmIK>();
            if (VRInstanceManager.modelPlayerHead != null && VRInstanceManager.modelPlayerHead.GetComponent<FixHeadRotation>() == null)
                VRInstanceManager.fixHeadRotationInstance = VRInstanceManager.modelPlayerHead.AddComponent<FixHeadRotation>();
            // Add vr capabilities to the weapons
            if (cameraScript.TargetCharacter.CurrentWeapon != null)
            {
                GameObject equippedVisualObject = cameraScript.TargetCharacter.CurrentWeapon.EquippedVisuals.gameObject;
                if (cameraScript.TargetCharacter.CurrentWeapon.Type == Weapon.WeaponType.FistW_2H && equippedVisualObject.GetComponent<VRFisticuffsHandler>() == null)
                    VRInstanceManager.vrWeaponController = equippedVisualObject.AddComponent<VRFisticuffsHandler>();
                else if (cameraScript.TargetCharacter.CurrentWeapon.EquippedVisuals.gameObject.GetComponent<VRMeleeHandler>() == null)
                    VRInstanceManager.vrWeaponController = equippedVisualObject.AddComponent<VRMeleeHandler>();
            }
            if (cameraScript.TargetCharacter.LeftHandWeapon != null)
            {
                GameObject equippedVisualObject = cameraScript.TargetCharacter.LeftHandWeapon.EquippedVisuals.gameObject;
                if (cameraScript.TargetCharacter.LeftHandWeapon.Type == Weapon.WeaponType.Shield && equippedVisualObject.GetComponent<VRShieldHandler>() == null)
                    VRInstanceManager.shieldHandlerInstance = equippedVisualObject.AddComponent<VRShieldHandler>();
                else if (cameraScript.TargetCharacter.LeftHandWeapon.Type == Weapon.WeaponType.Dagger_OH && equippedVisualObject.GetComponent<VRMeleeHandler>() == null)
                    VRInstanceManager.vrWeaponController = equippedVisualObject.AddComponent<VRMeleeHandler>();
            }
        }

        public static void SetFreezeMovement()
        {
            freezeMovement = true;
            freezeStartTime = Time.time;
        }

        public static void UnfreezeMovement()
        {
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
                camCurrentHeight = Camera.main.transform.localPosition.y;
                if (!VRInstanceManager.firstPerson)
                    HandleThirdPersonUpdate(__instance, ref ___m_inputMoveVector, ref ___m_modifMoveInput, ___m_horiControl, ref ___m_sprintFacing);
                else
                    HandleFirstPersonUpdate(__instance, ref ___m_inputMoveVector, ref ___m_modifMoveInput, ___m_horiControl, ref ___m_sprintFacing);
                return false;
            }
        }
        private static void HandleThirdPersonUpdate(LocalCharacterControl __instance, ref Vector3 ___m_inputMoveVector, ref Vector3 ___m_modifMoveInput,
           Transform ___m_horiControl, ref bool ___m_sprintFacing)
        {
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



            // ========= Mix of custom and default code to enable sideways and backwards movement =========
            // This allows the player to move side to side only if a menu isn't open and they're not in dialogue
            if (!m_char.CharacterUI.IsMenuFocused && !m_char.CharacterUI.IsDialogueInProgress)
                ___m_inputMoveVector.x += SteamVR_Actions._default.LeftJoystick.GetAxis(SteamVR_Input_Sources.Any).x / 2;

            // Moving backwards is pretty slow so increase it manually to speed it up. Set the if conditional to the inputMoveVector because the SteamVR input can be active during menus,
            // whereas inputMoveVector cant
            if (___m_inputMoveVector.y < -1)
                ___m_inputMoveVector.y = SteamVR_Actions._default.LeftJoystick.GetAxis(SteamVR_Input_Sources.Any).y * 5f;

            // Keep the movement from exceeding 10
            ___m_inputMoveVector.y = Mathf.Clamp(___m_inputMoveVector.y, -10, 10);
            ___m_inputMoveVector.x = Mathf.Clamp(___m_inputMoveVector.x, -10, 10);

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

                if (targetSys.Locked)
                {
                    __instance.transform.Rotate(0f, clampedDiff, 0f);
                    VRInstanceManager.camRoot.transform.rotation = __instance.transform.rotation;
                }
                else { 
                    clampedDiff += SteamVR_Actions._default.RightJoystick.axis.x * OptionManager.Instance.GetMouseSense(MenuManager.Instance.MapOwnerPlayerID);
                    VRInstanceManager.camRoot.transform.Rotate(0f, clampedDiff, 0f);
                
                    // Only rotate the body if the player isn't moving so we can get a full 360 degree view if we want
                    if (SteamVR_Actions._default.LeftJoystick.GetAxis(SteamVR_Input_Sources.Any).x != 0 || SteamVR_Actions._default.LeftJoystick.GetAxis(SteamVR_Input_Sources.Any).y != 0)
                        __instance.transform.rotation = Quaternion.Lerp(__instance.transform.rotation, VRInstanceManager.camRoot.transform.rotation, 5 * Time.deltaTime);
                }
                
            }

            if (Global.CheatsEnabled)
                m_char.NoFall = __instance.MovementMultiplier > 4f;

            __instance.m_localMovementVector = movementVector;
        }

        private static void HandleFirstPersonUpdate(LocalCharacterControl __instance, ref Vector3 ___m_inputMoveVector, ref Vector3 ___m_modifMoveInput,
        Transform ___m_horiControl, ref bool ___m_sprintFacing)
        {
            // Should only need the boolean but use the timer just in case something goes wrong and the boolean doesn't get flipped, so it unfreezes after 2 seconds
            if (freezeMovement && Time.time - freezeStartTime >= 2)
            {
                freezeMovement = false;
            }

            if (camInitYHeight == 0)
            {
                camInitYHeight = Camera.main.transform.localPosition.y;
                camTransform = Camera.main.transform;
                __instance.Character.RagdollRoot.gameObject.AddComponent<VRCrouch>();
            }

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
            camPosition.y = Camera.main.transform.localPosition.y * -1f;

            //if (__instance.Character.Sneaking)
            //    camPosition.y = Camera.main.transform.localPosition.y * -1f;
            //else
            //    camPosition.y = camInitYHeight * -1;
            Camera.main.transform.parent.localPosition = camPosition;

            // ========= Mix of custom and default code to enable sideways and backwards movement =========
            // This allows the player to move side to side only if a menu isn't open and they're not in dialogue


            if (VRInstanceManager.freezeCombat && freezeMovement)
            {
                ___m_inputMoveVector.x = 0;
                ___m_inputMoveVector.y = 0;
            }
            else
            {
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
                clampedDiff += SteamVR_Actions._default.RightJoystick.axis.x * OptionManager.Instance.GetMouseSense(MenuManager.Instance.MapOwnerPlayerID);
                __instance.transform.Rotate(0f, clampedDiff, 0f);
            }

            if (Global.CheatsEnabled)
                m_char.NoFall = __instance.MovementMultiplier > 4f;

            __instance.m_localMovementVector = movementVector;

        }
    }
}
