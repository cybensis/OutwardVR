using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace OutwardVR
{
    public class HandHandler : MonoBehaviour
    {



        private void LateUpdate() {
            if (this.name == "hand_left")
            {
                Vector3 localPos = CameraManager.LeftHand.transform.position - transform.position;
                float newZ = localPos.z;
                localPos.z = localPos.y;
                localPos.y = newZ;
                transform.position = CameraManager.LeftHand.transform.position;
                transform.localRotation = CameraManager.LeftHand.transform.localRotation;
            }
            else {
                Vector3 localPos = CameraManager.LeftHand.transform.position - transform.position;
                float newZ = localPos.z;
                localPos.z = localPos.y;
                localPos.y = newZ;
                transform.position = CameraManager.RightHand.transform.position;
                transform.localRotation = CameraManager.RightHand.transform.localRotation;

            }
        }

    }
}
