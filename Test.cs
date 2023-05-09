using OutwardVR.camera;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OutwardVR
{
    public class Test : MonoBehaviour
    {
        private float x, y, z;
        private Transform leftThigh;
        private Vector3 leftThighRot = new Vector3(20, 6, 47);
        private Transform leftCalf;
        private Vector3 leftCalfRot = new Vector3(-3, -6, -4);

        private Transform rightThigh;
        private Vector3 rightThighRot = new Vector3(5, -2, -46);
        private Transform rightCalf;
        private Vector3 rightCalfRot = new Vector3(-3, 14, 61);
        
        //private Vector3 pelvisRot = new Vector3(-1, +25, -7);
        private Vector3 pelvisPosition = new Vector3(-0.1f, -0.29f, -0.06f);

        private float crouchModifier = 0;


        void Awake() { 
            leftThigh = transform.GetChild(5);
            leftCalf = leftThigh.GetChild(0);
            rightThigh = transform.GetChild(6);
            rightCalf = rightThigh.GetChild(0);

            Logs.WriteError(FirstPersonCamera.camTransform.localPosition);
            Logs.WriteError(FirstPersonCamera.camInitYHeight);
        }

        void LateUpdate() {
            Logs.WriteWarning((FirstPersonCamera.camInitYHeight - FirstPersonCamera.camCurrentHeight) * 1.25f);
            //Logs.WriteError(Camera.main.transform.localPosition.y);
            crouchModifier = Mathf.Clamp(FirstPersonCamera.camInitYHeight - FirstPersonCamera.camCurrentHeight, 0, 1);

            //LT 20, 6, 47
            //LC -3 -6 -40
            //RT +5 -2 -46
            //RC -3 +14 +61
            //PE -1 +25 -7

            base.transform.localPosition += pelvisPosition * crouchModifier;

            leftThigh.Rotate(leftThighRot * crouchModifier);
            leftCalf.Rotate(leftCalfRot * crouchModifier);

            rightThigh.Rotate(rightThighRot * crouchModifier);
            rightCalf.Rotate(rightCalfRot * crouchModifier);



        }
    }
}
