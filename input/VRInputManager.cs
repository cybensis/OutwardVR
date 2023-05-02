using OutwardVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.Extras;

namespace OutwardVR;

public static class CameraManager
{
    public static SteamVR_LaserPointer laserPointer;
    static CameraManager()
    {


    }

    public static void Setup()
    {
        if (VROrigin == null)
        {
            VROrigin = new GameObject();
            VROrigin.transform.Rotate(0f, 270f, 0f, Space.Self);
        }
        //VROrigin.transform.parent = Camera.main.transform.parent.parent;
        SpawnHands();
        if (Camera.main != null)
        {
            if (RightHand)
                RightHand.transform.parent = Camera.main.transform.parent;
            if (LeftHand)
                LeftHand.transform.parent = Camera.main.transform.parent;
        }
    }

    public static void SpawnHands()
    {
        if (!RightHand)
        {
            //RightHand = GameObject.Instantiate(AssetLoader.RightHandBase, Vector3.zero, Quaternion.identity);
            RightHand = new GameObject("RightHand");
            RightHand.AddComponent<SteamVR_Behaviour_Pose>();
            //RightHand.AddComponent<SteamVR_Skeleton_Poser>();
            RightHand.transform.parent = VROrigin.transform;
            //RightHand.AddComponent<SteamVR_LaserPointer>();
            //RightHand.AddComponent<GraphicRaycaster>();
            //laserPointer = RightHand.GetComponent<SteamVR_LaserPointer>();
            //RightHand.AddComponent<LaserInteract>();
            //RightHand.AddComponent<SceneHandler>();

        }
        if (!LeftHand)
        {
            //LeftHand = GameObject.Instantiate(AssetLoader.LeftHandBase, Vector3.zero, Quaternion.identity);
            LeftHand = new GameObject("LeftHand");
            LeftHand.AddComponent<SteamVR_Behaviour_Pose>();
            LeftHand.transform.parent = VROrigin.transform;
        }
    }


    //public static void HandleFirstPersonCamera()
    //{
    //    if (CameraManager.CurrentCameraMode == CameraManager.VRCameraMode.FirstPerson)
    //    {
    //        // POSITION
    //        // Attach our origin to the Main Character's (this function gets called every tick)
    //        CameraManager.VROrigin.transform.position = Camera.main.GetComponent<CameraControl>().playerObj.transform.position;

    //        /*VROrigin.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);*/
    //        //VROrigin.transform.position = Game.Instance.Player.MainCharacter.Value.EyePosition;

    //        //ROTATION
    //        Vector3 RotationEulers = new Vector3(0, Turnrate * RightJoystick.x, 0);
    //        VROrigin.transform.Rotate(RotationEulers);

    //        // Movement is done via a patch
    //    }


    //}




    public enum VRCameraMode
    {
        DemeoLike,
        FirstPerson,
        Cutscene,
        UI
    }

    //Strictly camera stuff
    public static VRCameraMode CurrentCameraMode;
    public static float NearClipPlaneDistance = 0.001f;
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
