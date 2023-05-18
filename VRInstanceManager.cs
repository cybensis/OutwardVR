using OutwardVR.body;
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

        public static GameObject modelPlayerTorso;
        public static GameObject modelLeftHand;
        public static GameObject modelRightHand;

        public static ArmIK leftHandIK;
        public static ArmIK rightHandIK;

        public static Character characterInstance;

        public static bool gameHasBeenLoadedOnce = false;
        public static bool isLoading = false;

        public static bool headBobOn = false;


    }
}
