using OutwardVR.camera;
using UnityEngine;
using Valve.VR;
using static FightRecap;

namespace OutwardVR.combat
{
    public class VRFisticuffsHandler : MonoBehaviour
    {

        private float attackDelay = 0.4f;
        private float delayLength = 0f;
        private bool delayAttack = false;
        private float delayStartTime;

        private bool isSwinging = false;
        private bool swingFired = false;
        private float swingStart = 0f;
        private bool hasHit = false;
        private bool hitFired = false;
        RaycastHit hit;
        private float raycastLength = 0.2f;


        // The right hand kind of bends more to the side so it should have more range
        private const float RIGHT_HAND_BLOCKING_RANGE = -0.70f;
        private const float LEFT_HAND_BLOCKING_RANGE = 0.85f;
        private const float MAX_VELOCITY = 6f;
        private const float VELOCITY_THRESHOLD = 1.6f;

        // Used in the swing calculation where we divide this value by the current velocity minus the threshold velocity
        private const float MAX_VELOCITY_MINUS_THRESHOLD = MAX_VELOCITY - VELOCITY_THRESHOLD;


        private const float PUNCH_MAINTAINED_THRESHOLD = 0.12f;
        private const float PUNCH_MAINTAINED_MIN = 0.03f;
        // Threshold - min is what this value represents, this is the time difference allowed to be modified by reaching a velocity beyond threshold
        private const float PUNCH_VELOCITY_TIME_MODIFIER = PUNCH_MAINTAINED_THRESHOLD - PUNCH_MAINTAINED_MIN;

        private bool isBlocking = false;
        private GameObject leftHand;
        Character characterInstance;

        private Vector3 lastPosition = Vector3.zero;
        private void InitHand() {
            if (NetworkLevelLoader.Instance.IsOverallLoadingDone && NetworkLevelLoader.Instance.AllPlayerReadyToContinue)
            {
                leftHand = (GetComponent<ItemVisual>().m_item as DualMeleeWeapon).LeftHandFollow.gameObject;
                characterInstance = Camera.main.transform.root.GetComponent<Character>();
            }
        }

        private void Update()
        {
            if (leftHand == null || characterInstance == null)
                InitHand();

            if (delayAttack && Time.time - delayStartTime > delayLength) { 
                delayAttack = false;
                CameraHandler.UnfreezeMovement();
            }
            else if (delayAttack)
                return;

            float leftSwingVelocity = Mathf.Clamp(SteamVR_Actions._default.SkeletonLeftHand.velocity.magnitude, 0, MAX_VELOCITY);
            float rightSwingVelocity = Mathf.Clamp(SteamVR_Actions._default.SkeletonRightHand.velocity.magnitude, 0, MAX_VELOCITY);

            if (leftSwingVelocity < VELOCITY_THRESHOLD && rightSwingVelocity < VELOCITY_THRESHOLD)
            {
                float leftBlockingRange = Vector3.Dot(leftHand.transform.up, characterInstance.transform.up);
                float rightBlockingRange = Vector3.Dot(transform.forward, characterInstance.transform.up);
                // This is a little confusing because when the right hand is upwards, Vector3.Dot will return a negative so we need to check for less than,
                // but the left hand is the opposite and returns a positive so we check for greater than for the left hand
                if (rightBlockingRange < RIGHT_HAND_BLOCKING_RANGE && leftBlockingRange > LEFT_HAND_BLOCKING_RANGE)
                {
                    // Block here
                    characterInstance.BlockInput(true);
                    isBlocking = true;
                }
                else if (isBlocking && (rightBlockingRange >= RIGHT_HAND_BLOCKING_RANGE || leftBlockingRange <= LEFT_HAND_BLOCKING_RANGE))
                {
                    // Unblock here
                    characterInstance.BlockInput(false);
                    isBlocking = false;
                    if (WeaponPatches.hitWhileBlocking)
                    {
                        SetDelay(0.7f);
                        WeaponPatches.hitWhileBlocking = false;
                        CameraHandler.SetFreezeMovement();
                    }
                }
                else if (isSwinging)
                {
                    if (swingFired)
                        SetDelay(attackDelay);
                    resetSwingVariables();
                }
            }
            // The if will fail if either the left or right hand is above the velocity threshold, meaning they must be trying to punch
            else { 
                if (!isSwinging)
                {
                    resetSwingVariables();
                    swingStart = Time.time;
                    isSwinging = true;
                    characterInstance.NotifyNearbyAIOfAttack();
                }
                if (leftSwingVelocity > rightSwingVelocity)
                {
                    DetectPunch(leftSwingVelocity);
                    DetectHit(leftHand.transform.position, leftHand.transform.up, leftSwingVelocity);
                }
                else { 
                    DetectPunch(rightSwingVelocity);
                    DetectHit(transform.position, transform.forward * -1, rightSwingVelocity);
                }
            }
        }


        private void DetectPunch(float swingVelocity)
        {
            if (isSwinging && !swingFired && Time.time - swingStart >= PUNCH_MAINTAINED_THRESHOLD - PUNCH_VELOCITY_TIME_MODIFIER * ((swingVelocity - VELOCITY_THRESHOLD) / MAX_VELOCITY_MINUS_THRESHOLD))
            {
                swingFired = true;
                if (characterInstance.HasEnoughStamina(characterInstance.CurrentWeapon.StamCost))
                {
                    // Figure out what the attack ID's for different combos are and for heavy attacks then manually set these values here or something
                    int attackType = SteamVR_Actions._default.ButtonA.stateDown ? 0 : 1;
                    characterInstance.AttackInput(attackType, characterInstance.m_nextAttackID);
                    characterInstance.HitStarted(attackType);
                }
            }
        }

        private void DetectHit(Vector3 punchingHandPos, Vector3 direction, float swingVelocity)
        {

            if (isSwinging && !hasHit && Physics.Raycast(punchingHandPos, direction, out hit, raycastLength, LayerMask.GetMask("Hitbox")) && !hit.collider.GetComponent<Hitbox>().m_ownerChar.IsLocalPlayer)
                hasHit = true;
            if (swingFired && hasHit && !hitFired && characterInstance.HasEnoughStamina(characterInstance.CurrentWeapon.StamCost))
            {
                hitFired = true;
                // Try and add direction here cos I think it'll make enemies bodies ragdoll in that direction if they die
                characterInstance.CurrentWeapon.HasHit(hit, (punchingHandPos - lastPosition) * 3);
            }
            // Update the last position only after we've checked for a hit
            if (isSwinging && swingVelocity >= VELOCITY_THRESHOLD)
            {
                lastPosition = punchingHandPos;
            }
        }

        private void resetSwingVariables()
        {
            swingFired = false;
            hasHit = false;
            hitFired = false;
            isSwinging = false;
        }

        private void SetDelay(float timeToDelay)
        {
            delayLength = timeToDelay;
            delayStartTime = Time.time;
            delayAttack = true;
        }
    }
}
