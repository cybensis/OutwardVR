using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Rewired;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Valve.VR;
using static MapMagic.ObjectPool;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.Management;
using Unity.XR.OpenVR;


namespace OutwardVR
{
    [HarmonyPatch]
    internal class CameraPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainScreen), nameof(MainScreen.StartInit))]
        private static void OnCameraRigEnabled()
        {
            Logs.WriteInfo("CameraRig OnEnable started");


            //Without this there is no headtracking
            Camera.main.gameObject.AddComponent<SteamVR_TrackedObject>();

            //Plugin.SecondEye = new GameObject("SecondEye");
            //Plugin.SecondCam = Plugin.SecondEye.AddComponent<Camera>();
            //Plugin.SecondCam.gameObject.AddComponent<SteamVR_TrackedObject>();
            //Plugin.SecondCam.CopyFrom(Camera.main);

            ////// Without this the right eye gets stuck at a very far point in the map
            //Plugin.SecondCam.transform.parent = Camera.main.transform.parent;



        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(MainScreen), nameof(MainScreen.Update))]
        //private static void HandleCamera()
        //{
        //    if (Camera.main != null)
        //    {
        //        Logs.WriteWarning("UPDATE");
        //        //Camera.main.fieldOfView = SteamVR.instance.fieldOfView;
        //        Camera.main.stereoTargetEye = StereoTargetEyeMask.Both;
        //        //Camera.main.projectionMatrix = Camera.main.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
        //        //Camera.main.targetTexture = Plugin.MyDisplay.GetRenderTextureForRenderPass(0);

        //        Plugin.SecondEye.transform.position = Camera.main.transform.position;
        //        Plugin.SecondEye.transform.rotation = Camera.main.transform.rotation;
        //        Plugin.SecondEye.transform.localScale = Camera.main.transform.localScale;
        //        Plugin.SecondCam.enabled = true;
        //        Plugin.SecondCam.stereoTargetEye = StereoTargetEyeMask.Right;
        //        //Plugin.SecondCam.projectionMatrix = Plugin.SecondCam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
        //        //Plugin.SecondCam.targetTexture = Plugin.MyDisplay.GetRenderTextureForRenderPass(1);
        //    }
        //}


    }
}
