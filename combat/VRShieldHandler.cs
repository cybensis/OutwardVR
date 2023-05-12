using OutwardVR.camera;
using UnityEngine;
using Valve.VR;

namespace OutwardVR.combat
{
    internal class VRShieldHandler : MonoBehaviour
    {
        private float x, y, z;
        private Character characterInstance;
        private const float VELOCITY_THRESHOLD = 1.6f;
        private const float MAX_VELOCITY = 6f;
        private bool isBlocking = false;
        private const float UP_SHIELD_MIN_BLOCK_RANGE = 0.8f;
        private const float SIDEWAYS_SHIELD_MIN_BLOCK_RANGE = 0.75f;

        private const float BLOCK_DELAY = 0.45f;
        private float delayLength = 0f;
        private bool delayAttack = false;
        private float delayStartTime;
        private void Awake() {
            if (NetworkLevelLoader.Instance.IsOverallLoadingDone && NetworkLevelLoader.Instance.AllPlayerReadyToContinue) {
                characterInstance = Camera.main.transform.root.GetComponent<Character>();
            }
        }

        private void Update() {
            if (delayAttack && Time.time - delayStartTime > delayLength) { 
                delayAttack = false;
                FirstPersonCamera.UnfreezeMovement();
            }
            else if (delayAttack)
                return;
            // We still want to check the right hand velocity to see if the player is swinging something around 
            float swingVelocity = Mathf.Clamp(SteamVR_Actions._default.SkeletonRightHand.velocity.magnitude, 0, MAX_VELOCITY);
            if (swingVelocity < VELOCITY_THRESHOLD)
            {
                // If you rotate the shield 90 degrees on Y it still blocks so maybe add another range check for a difference axis
                float upBlockingRange = Vector3.Dot(transform.right, characterInstance.transform.up);
                float sidewaysBlockingRange = Vector3.Dot(transform.forward, characterInstance.transform.right);
                if (upBlockingRange >= UP_SHIELD_MIN_BLOCK_RANGE && sidewaysBlockingRange >= SIDEWAYS_SHIELD_MIN_BLOCK_RANGE)
                {
                    characterInstance.BlockInput(true);
                    isBlocking = true;
                }
                else if (isBlocking && (upBlockingRange < UP_SHIELD_MIN_BLOCK_RANGE || sidewaysBlockingRange < SIDEWAYS_SHIELD_MIN_BLOCK_RANGE))
                {
                    characterInstance.BlockInput(false);
                    isBlocking = false;
                    if (WeaponPatches.hitWhileBlocking)
                    {
                        SetDelay(BLOCK_DELAY);
                        WeaponPatches.hitWhileBlocking = false;
                    }
                }
            }
            else if (swingVelocity >= VELOCITY_THRESHOLD && isBlocking && WeaponPatches.hitWhileBlocking)
            {
                SetDelay(BLOCK_DELAY);
                WeaponPatches.hitWhileBlocking = false;
            }
        }


        private void SetDelay(float timeToDelay)
        {
            delayLength = timeToDelay;
            delayStartTime = Time.time;
            delayAttack = true;
            FirstPersonCamera.SetFreezeMovement();
        }
    }
}
