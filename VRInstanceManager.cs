using OutwardVR.body;
using OutwardVR.camera;
using OutwardVR.combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OutwardVR
{
    internal static class VRInstanceManager
    {

        public static GameObject modelPlayerHead;
        public static GameObject nonBobPlayerHead;
        public static SkinnedMeshRenderer playerHair;
        public static SkinnedMeshRenderer activeVisualsHelmOrHead;

        public static GameObject modelPlayerTorso;
        public static GameObject modelLeftHand;
        public static GameObject modelRightHand;

        public static Character characterInstance;

        public static GameObject camRoot;

        public static bool gameHasBeenLoadedOnce = false;
        public static bool isLoading = false;

        public static bool headBobOn = false;
        public static bool freezeCombat = false;
        public static bool firstPerson = true;
        public static bool moveFromHeadset = false;

        public static bool gamepadInUse = false;


        public static int currentPlayerId = 0;


        // First person elements
        public static ArmIK leftHandIK;
        public static ArmIK rightHandIK;
        public static FixHeadRotation fixHeadRotationInstance;
        public static MonoBehaviour vrWeaponController;
        public static VRShieldHandler  shieldHandlerInstance;
        public static VRCrouch crouchInstance;
        private static float timeLastToggled = 0;

        public static void ToggleHeadBob() { 
            headBobOn = !headBobOn;
            if (VRInstanceManager.firstPerson && modelPlayerHead != null)
            {
                if (headBobOn)
                   camRoot.transform.SetParent(modelPlayerHead.transform, false);
                else
                    camRoot.transform.SetParent(nonBobPlayerHead.transform, false);
            }
        }


        public static void ToggleThirdPerson()
        {
            if (Time.time - timeLastToggled > 2.5f) { 
                timeLastToggled = Time.time;
                firstPerson = !firstPerson;
                ToggleMotionControls((gamepadInUse) ? false : firstPerson ? true : false);
                if (crouchInstance != null)
                    crouchInstance.enabled = firstPerson;
                if (fixHeadRotationInstance != null)
                    fixHeadRotationInstance.enabled = firstPerson;
                if (VRInstanceManager.playerHair != null)
                    VRInstanceManager.playerHair.enabled = !firstPerson;
                if (VRInstanceManager.activeVisualsHelmOrHead != null)
                    VRInstanceManager.activeVisualsHelmOrHead.enabled = !firstPerson;
                // Use this to re-run FixCamera
                CameraHandler.cameraFixed = false;
            }
        }

        public static void ToggleMotionControls(bool enable) {
            if (leftHandIK != null)
                leftHandIK.enabled = enable;
            if (rightHandIK != null)
                rightHandIK.enabled = enable;
            if (vrWeaponController != null)
                vrWeaponController.enabled = enable;
            if (shieldHandlerInstance != null)
                shieldHandlerInstance.enabled = enable;

        }


        public static void SetGamepad(bool useGamepad)
        {
            if (firstPerson && gamepadInUse != useGamepad) {
                ToggleMotionControls(!useGamepad);
            }
            gamepadInUse = useGamepad;

        }


    }
}
