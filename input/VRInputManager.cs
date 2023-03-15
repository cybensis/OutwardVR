using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace OutwardVR
{
    public class VRInputManager
    {
        static VRInputManager()
        {
            SetUpListeners();
        }

        public static void SetUpListeners()
        {
            // BOOLEANS

            // VECTOR 2Ds
            SteamVR_Actions._default.LeftJoystick.AddOnUpdateListener(OnLeftJoystickUpdate, SteamVR_Input_Sources.Any);
            SteamVR_Actions._default.RightJoystick.AddOnUpdateListener(OnRightJoystickUpdate, SteamVR_Input_Sources.Any);

            // POSES
           /* SteamVR_Actions._default.RightHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, UpdateRightHand);
            SteamVR_Actions._default.LeftHandPose.AddOnUpdateListener(SteamVR_Input_Sources.Any, UpdateLeftHand);*/
        }


        // BOOLEANS
 /*       public static void OnSwitchPOVDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        {
            CameraManager.SwitchPOV();
            CameraManager.SpawnHands();
        }
*/
        //public static void TriggerLeftDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
        //{
        //    //Logs.WriteInfo("TriggerLeft is Down");
        //    LogBinds();
        //}

 
        // VECTOR 2Ds
        public static void OnLeftJoystickUpdate(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
        {
            // Doesn't seem to stop joystick drift in it's current state?
            if (axis.magnitude > 0.1f)
                CameraManager.LeftJoystick = axis;
            else
                CameraManager.LeftJoystick = Vector2.zero;
        }

        public static void OnRightJoystickUpdate(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 delta)
        {
            // Doesn't seem to stop joystick drift in it's current state?
            if (axis.magnitude > 0.1f)
                CameraManager.RightJoystick = axis;
            else
                CameraManager.RightJoystick = Vector2.zero;
        }


        // POSES
        public static void UpdateRightHand(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
        {
            if (CameraManager.RightHand)
            {
                Logs.WriteInfo(fromAction.localPosition);
                CameraManager.RightHand.transform.localPosition = fromAction.localPosition;
                CameraManager.RightHand.transform.localRotation = fromAction.localRotation;
            }

        }

        public static void UpdateLeftHand(SteamVR_Action_Pose fromAction, SteamVR_Input_Sources fromSource)
        {
            if (CameraManager.LeftHand)
            {
                CameraManager.LeftHand.transform.localPosition = fromAction.localPosition;
                CameraManager.LeftHand.transform.localRotation = fromAction.localRotation;
            }
        }


        //// GAMEPAD STUFF

        //public static void LogBinds()
        //{
        //    //Controllers.LogAllGameActions(Kingmaker.Assets.Console.GamepadInput.GamePad.Instance.Player);
        //}
    }
}
