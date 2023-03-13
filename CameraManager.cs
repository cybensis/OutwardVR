using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace OutwardVR
{
    public static class CameraManager
    {
        static CameraManager()
        {
            CurrentCameraMode = VRCameraMode.UI;
            //Fix near plance clipping for main camera
            if (Camera.main != null)
            {
                Camera.main.nearClipPlane = NearClipPlaneDistance;
                Camera.main.farClipPlane = FarClipPlaneDistance;
            }

        }

        /*  public static void SwitchPOV()
          {

              Camera OriginalCamera = Camera.main;
              if (VROrigin == null) {
                  VROrigin = new GameObject();
              }
              // If we are not in firstperson
              if (CameraManager.CurrentCameraMode != CameraManager.VRCameraMode.FirstPerson)
              {
                  if (Camera.main != null)
                  {
                      // switch to first person
                      Logs.WriteInfo("TEST");
                      VROrigin.transform.parent = null;
                      Logs.WriteInfo("TEST");
                      Logs.WriteInfo(Camera.main);
                    Logs.WriteInfo(Camera.main.GetComponent<CharacterCamera>());
                      Logs.WriteInfo(Camera.main.GetComponentInParent<CharacterCamera>());
                      Logs.WriteInfo(Camera.main.GetComponentInParent<CharacterCamera>().TargetCharacter);
                      VROrigin.transform.position = Camera.main.GetComponentInParent<CharacterCamera>().TargetCharacter.transform.position;

                      //VROrigin.transform.LookAt(Game.Instance.Player.MainCharacter.Value.OrientationDirection);

                      if (!OriginalCameraParent)
                      {
                          OriginalCameraParent = OriginalCamera.transform.parent;
                      }

                      OriginalCamera.transform.parent = VROrigin.transform;
                      if (RightHand)
                          RightHand.transform.parent = VROrigin.transform;
                      if (LeftHand)
                          LeftHand.transform.parent = VROrigin.transform;
                      CameraManager.CurrentCameraMode = CameraManager.VRCameraMode.FirstPerson;
                  }

              }
              else
              {
                  VROrigin.transform.position = OriginalCameraParent.position;
                  VROrigin.transform.rotation = OriginalCameraParent.rotation;
                  VROrigin.transform.localScale = OriginalCameraParent.localScale;

                  VROrigin.transform.parent = OriginalCameraParent;

                  CameraManager.CurrentCameraMode = CameraManager.VRCameraMode.DemeoLike;
              }
          }*/


        public static void SwitchPOV()
        {
            Logs.WriteInfo("Entered SwitchPOV function");

            Logs.WriteInfo("AddedSkyBox");
            if (VROrigin == null)
            {
                VROrigin = new GameObject();
            }

         /*   Camera OriginalCamera = Camera.main;
            OriginalCameraParent = OriginalCamera.transform.parent;
            OriginalCamera.transform.parent = VROrigin.transform;
            VROrigin.transform.position = OriginalCameraParent.position;
            VROrigin.transform.rotation = OriginalCameraParent.rotation;
            VROrigin.transform.localScale = OriginalCameraParent.localScale;
            VROrigin.transform.parent = OriginalCameraParent;*/

            CameraManager.CurrentCameraMode = CameraManager.VRCameraMode.DemeoLike;
        }
        public static void SpawnHands()
        {
            if (!RightHand)
            {
                RightHand = GameObject.Instantiate(AssetLoader.RightHandBase, Vector3.zeroVector, Quaternion.identityQuaternion);
                RightHand.transform.parent = VROrigin.transform;
            }
            if (!LeftHand)
            {
                LeftHand = GameObject.Instantiate(AssetLoader.LeftHandBase, Vector3.zeroVector, Quaternion.identityQuaternion);
                LeftHand.transform.parent = VROrigin.transform;
            }
        }

        public static void HandleDemeoCamera()
        {
            if ((CameraManager.CurrentCameraMode == CameraManager.VRCameraMode.DemeoLike)
                && RightHand && LeftHand)
            {
                // Add physics to the VROrigin
                /*    if (!VROrigin.GetComponent<Rigidbody>())
                    {
                        Rigidbody tempvar = VROrigin.AddComponent<Rigidbody>();
                        tempvar.useGravity = false;
                    }    
                    Rigidbody VROriginPhys = VROrigin.GetComponent<Rigidbody>();*/

            }
        }

        public static void HandleFirstPersonCamera()
        {
            if (CameraManager.CurrentCameraMode == CameraManager.VRCameraMode.FirstPerson)
            {
                // POSITION
                // Attach our origin to the Main Character's (this function gets called every tick)
                CameraManager.VROrigin.transform.position = Camera.main.GetComponentInParent<CharacterCamera>().TargetCharacter.transform.position;
                //VROrigin.transform.position = Game.Instance.Player.MainCharacter.Value.EyePosition;

                //ROTATION
                Vector3 RotationEulers = new Vector3(0, Turnrate * RightJoystick.x, 0);
                VROrigin.transform.Rotate(RotationEulers);

                // Movement is done via a patch
            }


        }

        public enum VRCameraMode
        {
            DemeoLike,
            FirstPerson,
            Cutscene,
            UI
        }

        //Strictly camera stuff
        public static VRCameraMode CurrentCameraMode;
        public static float NearClipPlaneDistance = 0.01f;
        public static float FarClipPlaneDistance = 59999f;
        public static bool DisableParticles = false;

        // VR Origin and body stuff
        public static Transform OriginalCameraParent = null;
        public static GameObject VROrigin = new GameObject();
        public static GameObject LeftHand = null;
        public static GameObject RightHand = null;

        // VR Input stuff
        public static bool RightHandGrab = false;
        public static bool LeftHandGrab = false;
        public static Vector2 LeftJoystick = Vector2.zero;
        public static Vector2 RightJoystick = Vector2.zero;

        // Demeo-like camera stuff
        public static float InitialHandDistance = 0f;
        public static bool InitialRotation = true;
        public static Vector3 PreviousRotationVector = Vector3.zero;
        public static Vector3 InitialRotationPoint = Vector3.zero;
        public static Vector3 ZoomOrigin = Vector3.zero;
        public static float SpeedScalingFactor = 1f;

        // FIrst person camera stuff
        public static float Turnrate = 3f;

        //SKybox stuff
        public static GameObject SceneSkybox = null;

    }

}
