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
            transform.localPosition = CameraManager.LeftHand.transform.localPosition - new Vector3(0, 0.6f, 0.75f);
            transform.localRotation = CameraManager.LeftHand.transform.localRotation;
        }

    }
}
