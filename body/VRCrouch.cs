using OutwardVR.camera;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace OutwardVR.body
{
    public class VRCrouch : MonoBehaviour
    {

        private Transform leftThigh;
        private Vector3 leftThighRot = new Vector3(20, 6, 47);
        private Transform leftCalf;
        private Vector3 leftCalfRot = new Vector3(-3, -6, -4);

        private Transform rightThigh;
        private Vector3 rightThighRot = new Vector3(5, -2, -46);
        private Transform rightCalf;
        private Vector3 rightCalfRot = new Vector3(-3, 14, 61);

        //private Vector3 pelvisRot = new Vector3(-1, +25, -7);
        //private Vector3 pelvisPosition = new Vector3(-0.1f, -0.29f, -0.06f);
        private Vector3 pelvisPosition = new Vector3(-0.1f, -0.29f, -0.06f);

        private float crouchModifier = 0;

        private bool actualCrouchEnabled = false;


        void Awake()
        {
            leftThigh = transform.GetChild(5);
            leftCalf = leftThigh.GetChild(0);
            rightThigh = transform.GetChild(6);
            rightCalf = rightThigh.GetChild(0);
        }

        void LateUpdate()
        {
            crouchModifier = Mathf.Clamp(FirstPersonCamera.camInitYHeight - FirstPersonCamera.camCurrentHeight, 0, 1) * 1.25f;

            //LT 20, 6, 47
            //LC -3 -6 -40
            //RT +5 -2 -46
            //RC -3 +14 +61
            //PE -1 +25 -7
            //if (!actualCrouchEnabled && crouchModifier >= 0.6f)
            if (!actualCrouchEnabled && crouchModifier >= 1f)
            {
                MiscPatches.characterUIInstance.m_targetCharacter.StealthInput(true);
                actualCrouchEnabled = true;
            }
            else if (actualCrouchEnabled && crouchModifier < 1f)
            //else if (actualCrouchEnabled && crouchModifier < 0.6f)
            {
                MiscPatches.characterUIInstance.m_targetCharacter.StealthInput(false);
                actualCrouchEnabled = false;
            }
            if (!actualCrouchEnabled) { 
                transform.localPosition += pelvisPosition * crouchModifier;

                leftThigh.Rotate(leftThighRot * crouchModifier);
                leftCalf.Rotate(leftCalfRot * crouchModifier);

                rightThigh.Rotate(rightThighRot * crouchModifier);
                rightCalf.Rotate(rightCalfRot * crouchModifier);
            }



        }
    }
}
