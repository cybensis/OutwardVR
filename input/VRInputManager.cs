using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;

namespace OutwardVR;

public static class CameraManager
{
    public static SteamVR_LaserPointer laserPointer;

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

        }
        if (!LeftHand)
        {
            //LeftHand = GameObject.Instantiate(AssetLoader.LeftHandBase, Vector3.zero, Quaternion.identity);
            LeftHand = new GameObject("LeftHand");
            LeftHand.AddComponent<SteamVR_Behaviour_Pose>();
            LeftHand.transform.parent = VROrigin.transform;
        }
    }




    public enum VRCameraMode
    {
        DemeoLike,
        FirstPerson,
        Cutscene,
        UI
    }


    // VR Origin and body stuff
    public static Transform OriginalCameraParent = null;
    public static GameObject VROrigin = new GameObject();
    public static GameObject LeftHand = null;
    public static GameObject RightHand = null;


}
