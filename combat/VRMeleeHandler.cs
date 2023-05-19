using OutwardVR.body;
using OutwardVR.camera;
using UnityEngine;
using UnityEngine.TextCore;
using Valve.VR;

namespace OutwardVR.combat
{
    public class VRMeleeHandler : MonoBehaviour
    {

        private float x, y, z;

        private static float BASE_WEAPON_REACH = 4.8f;
        private static float BASE_RAYCAST_LENGTH = 0.75f;


        // To be registered as swinging, the swing needs to reach a velocity of 1.6 which is kind of slow
        // but players shouldn't have to swing fast to actualy swing (going on saints and sinners logic)
        private const float VELOCITY_THRESHOLD = 1.6f;
        // A swing at the threshold velocity needs to be maintained for 0.35f seconds for it to be a full swing
        private const float SWING_MAINTAINED_THRESHOLD = 0.35f;
        // No matter how fast the player swings, it must be maintained for atleast 0.08 seconds
        private const float SWING_MAINTAINED_MIN = 0.08f;
        // Threshold - min is what this value represents, this is the time difference allowed to be modified by reaching a velocity beyond threshold
        private const float SWING_VELOCITY_TIME_MODIFIER = SWING_MAINTAINED_THRESHOLD - SWING_MAINTAINED_MIN;
        // A velocity of 6 is pretty fast, players really shouldn't go beyond this
        private const float MAX_VELOCITY = 6f;
        // Used in the swing calculation where we divide this value by the current velocity minus the threshold velocity
        private const float MAX_VELOCITY_MINUS_THRESHOLD = MAX_VELOCITY - VELOCITY_THRESHOLD;


        // Used to tell if the velocity has exceeded the threshold
        private bool isSwinging = false;
        // Once the minimum time for a swing has been reached, the swing is fired
        private bool swingFired = false;
        // The time when the swing started
        private float swingStart = 0f;
        // If the raycast hits a hitbox, this bool gets set
        private bool hasHit = false;
        // Once a hit has been found and the minimum swing time has been reached, the hit is fired and this gets set
        private bool hitFired = false;
        private bool isBlocking = false;
        RaycastHit hit;
        private float raycastLength = 0f;
        Character characterInstance;
        private ArmIK handIK;

        // When the sword is held to the side to block, it should be within range of 1.0 to SWORD_MIN_BLOCK_RANGE otherwise its not gunna count as a block
        private const float SWORD_MIN_BLOCK_RANGE = 0.875f;
        private const float BLOCK_DELAY = 0.45f;
        private float attackDelay = 0.45f;
        private float delayLength = 0f;
        private bool delayAttack = false;
        private float delayStartTime;

        private const float STAB_MAINTAINED_THRESHOLD = 0.15f;
        private const float STAB_MAINTAINED_MIN = 0.05f;
        // Threshold - min is what this value represents, this is the time difference allowed to be modified by reaching a velocity beyond threshold
        private const float STAB_VELOCITY_TIME_MODIFIER = STAB_MAINTAINED_THRESHOLD - STAB_MAINTAINED_MIN;
        private bool possibleStab = true;

        private Weapon weaponInstance;

        private const float STAB_POSITION_RANGE = 0.8f;

        // When triggering the Hit function, the direction of the swing is passed in but it needs to be multiplied by a modifier to make it actually do something
        private float weaponDirectionModifier = 5;

        private Vector3 lastPosition = Vector3.zero;

        private bool isLeftHand = false;


        private void OnDisable() {
            // Need to do this so it reruns InitVars when enabled again, but cant use OnEnable because the weapon won't be equipped when its ran so just use the InitVars call in Update()
            handIK = null;
        }

        private void InitVars()
        {
            if (handIK == null)
                handIK = transform.parent.parent.parent.GetComponent<ArmIK>();

            if (weaponInstance == null)
                weaponInstance = GetComponent<ItemVisual>().m_item as MeleeWeapon;

            if (characterInstance == null)
                characterInstance = weaponInstance.OwnerCharacter;

            if (handIK.name == "hand_left")
                isLeftHand = true;
            if (isLeftHand && characterInstance.LeftHandWeapon.CurrentVisual.name != name)
                return;
            else if (!isLeftHand && characterInstance.CurrentWeapon.CurrentVisual.name != name)
                return;

            // The two BASE vars were made based on the iron swords reach, so by dividing the reach of the current weapon by the iron swords reach, 
            // we get a modifier value to multiply the raycast length by.
            raycastLength = weaponInstance.Reach / BASE_WEAPON_REACH * BASE_RAYCAST_LENGTH;

            if (weaponInstance.Type == Weapon.WeaponType.Halberd_2H ||
                weaponInstance.Type == Weapon.WeaponType.Sword_2H ||
                weaponInstance.Type == Weapon.WeaponType.Axe_2H ||
                weaponInstance.Type == Weapon.WeaponType.Mace_2H ||
                weaponInstance.Type == Weapon.WeaponType.Spear_2H
            ) { 
                attackDelay = 0.7f;
                weaponDirectionModifier = 7.5f;
            }
            else if (weaponInstance.Type == Weapon.WeaponType.Dagger_OH) { 
                attackDelay = 0.2f;
                weaponDirectionModifier = 3.5f;
            }
            if (isLeftHand)
                return;
            if (weaponInstance.TwoHanded)
                raycastLength += 0.1f;

            if (weaponInstance.Type == Weapon.WeaponType.Axe_2H || weaponInstance.Type == Weapon.WeaponType.Mace_2H)
                transform.parent.localPosition = new Vector3(0f, -0.3f, 0f);
            else if (weaponInstance.Type == Weapon.WeaponType.Halberd_2H)
            {
                transform.parent.localPosition = new Vector3(0f, -0.6f, 0f);
                raycastLength += 0.1f;
            }
            else if (weaponInstance.Type == Weapon.WeaponType.Spear_2H)
            {
                transform.parent.localPosition = new Vector3(0f, -0.8f, 0f);
                raycastLength += 0.2f;
            }
            else
                transform.parent.localPosition = Vector3.zero;

        }


        void Update()
        {
            if (!NetworkLevelLoader.Instance.IsOverallLoadingDone || !NetworkLevelLoader.Instance.AllPlayerReadyToContinue || !gameObject.GetActive())
                return;
            if (handIK == null || characterInstance == null || raycastLength == 0f)
                InitVars();
            if (delayAttack && Time.time - delayStartTime > delayLength) { 
                delayAttack = false;
                CameraHandler.UnfreezeMovement();
            }
            else if (delayAttack)
                return;
            if (characterInstance.LeftHandWeapon != null && characterInstance.LeftHandWeapon.Type == Weapon.WeaponType.Shield && characterInstance.Blocking)
                return;
            

            float swingVelocity;
            if (isLeftHand)
                swingVelocity = Mathf.Clamp(SteamVR_Actions._default.SkeletonLeftHand.velocity.magnitude, 0, MAX_VELOCITY);
            else
                swingVelocity = Mathf.Clamp(SteamVR_Actions._default.SkeletonRightHand.velocity.magnitude, 0, MAX_VELOCITY);
            // Only call the slash/hit/stab functions if the velocity is over the threshold, otherwise its wasting resources
            if (swingVelocity < VELOCITY_THRESHOLD)
                DetectBlock(swingVelocity);
            else { 
                DetectSlash(swingVelocity);
                DetectStab(swingVelocity);
                DetectHit(swingVelocity);
            }


            if (swingVelocity >= VELOCITY_THRESHOLD && !isSwinging)
            {
                resetSwingVariables();
                swingStart = Time.time;
                isSwinging = true;
                possibleStab = true;
                // When a player starts to swing notifiy enemies as early as possible so they can dodge. This is a limitation of the VR mod since
                // the normal time to attack is pretty slow so AI migh never actually be able to dodge attacks due to the speed of VR combat.
                characterInstance.NotifyNearbyAIOfAttack();
            }
            else if (swingVelocity < VELOCITY_THRESHOLD && isSwinging)
            {
                if (swingFired)
                    SetDelay(attackDelay);
                resetSwingVariables();
            }
        }


        private void DetectHit(float swingVelocity) {
            Vector3 raycastDirection = transform.right * -1;
                 
            if (isSwinging && !hasHit && Physics.Raycast(handIK.transform.position, raycastDirection, out hit, raycastLength, LayerMask.GetMask("Hitbox")) && !hit.collider.GetComponent<Hitbox>().m_ownerChar.IsLocalPlayer)
                hasHit = true;
            if (swingFired && hasHit && !hitFired && characterInstance.HasEnoughStamina(weaponInstance.StamCost)) {
                hitFired = true;
                // Try and add direction here cos I think it'll make enemies bodies ragdoll in that direction if they die
                //Vector3 direction = (SteamVR_Actions._default.RightHandPose.localPosition - SteamVR_Actions._default.RightHandPose.lastLocalPosition) * 4;
                Vector3 direction = (handIK.transform.position - lastPosition) * weaponDirectionModifier;
                weaponInstance.HasHit(hit, direction);
            }
            // Update the last position only after we've checked for a hit
            if (isSwinging && swingVelocity >= VELOCITY_THRESHOLD) { 
                lastPosition = handIK.transform.position;
            }
        }


        private void DetectBlock(float swingVelocity) {
            // This checks if the swords right is pointed in the same direction as the bodies right, if its == 1 then its pointing exactly the same direction
            // but we want some leeway so the sword doesn't have to be h,eld exactly at the bodies right.
            float blockingRange = Mathf.Abs(Vector3.Dot(transform.right, characterInstance.transform.right));
            if (blockingRange >= SWORD_MIN_BLOCK_RANGE)
            {
                // BlockInput is called with the argument _active (blocking is or isn't active), which calls on SendBlocKStateTrivial and if _active is true
                // it calls on StartBlocking and that sets blocking as true, otherwise it calls StopBlocking which sets blocking to false.
                characterInstance.BlockInput(true);
                isBlocking = true;
            }
            else if (isBlocking && blockingRange < SWORD_MIN_BLOCK_RANGE)
            {
                // Unblock here
                characterInstance.BlockInput(false);
                isBlocking = false;
                // Only delay after blocking if the player has been hit, since blocking can easily be triggered by accident
                if (WeaponPatches.hitWhileBlocking)
                {
                    SetDelay(BLOCK_DELAY);
                    WeaponPatches.hitWhileBlocking = false;
                }
            }
            // The player can come out of a block swinging above the threshold so we need this to catch that and delay them if need be
            if (swingVelocity >= VELOCITY_THRESHOLD && isBlocking && WeaponPatches.hitWhileBlocking)
            {
                SetDelay(BLOCK_DELAY);
                WeaponPatches.hitWhileBlocking = false;
            }
        }


        private void DetectSlash(float swingVelocity)
        {
            // Using the constants explained above, we check if the time swung for is greater than or equal to the threshold swing time, with a modified value for extended beyond the threshold velocity
            // The max velocitiy minus the threshold value, 4.65f, divided by the swingvelocity capped at 6f minus the threshold velocity will return a number less than or equal to 1, then this is used
            // by multiplying it against the time modifier which is then substracted from the maintained swing time threshold. E.g. if we have a swing of velocity 6, 4.65 / (6 - 1.35) will be 1, then 1 * 0.23 
            // will obviously be 0.23, then the maintained swing time threshold value minus 0.23 is 0.12 which is the minimum time we allow for something to count as a swing even at the max velocity.
            if (isSwinging && !swingFired && Time.time - swingStart >= SWING_MAINTAINED_THRESHOLD - SWING_VELOCITY_TIME_MODIFIER * ((swingVelocity - VELOCITY_THRESHOLD) / MAX_VELOCITY_MINUS_THRESHOLD))
            {
                swingFired = true;
                if (characterInstance.HasEnoughStamina(weaponInstance.StamCost))
                {
                    // Figure out what the attack ID's for different combos are and for heavy attacks then manually set these values here or something
                    int attackType = SteamVR_Actions._default.ButtonA.stateDown ? 0 : 1;
                    characterInstance.AttackInput(attackType, characterInstance.m_nextAttackID);
                    characterInstance.HitStarted(attackType);
                }
            }
        }


        private void DetectStab(float swingVelocity) {
            if (possibleStab && Mathf.Abs(Vector3.Dot(transform.right, characterInstance.transform.forward)) < STAB_POSITION_RANGE)
                possibleStab = false;

            if (!swingFired && possibleStab && Time.time - swingStart >= STAB_MAINTAINED_THRESHOLD - STAB_VELOCITY_TIME_MODIFIER * ((swingVelocity - VELOCITY_THRESHOLD) / MAX_VELOCITY_MINUS_THRESHOLD))
            {
                swingFired = true;
                if (characterInstance.HasEnoughStamina(weaponInstance.StamCost))
                {
                    // Figure out what the attack ID's for different combos are and for heavy attacks then manually set these values here or something
                    int attackType = SteamVR_Actions._default.ButtonA.stateDown ? 0 : 1;
                    characterInstance.AttackInput(attackType, characterInstance.m_nextAttackID);
                    characterInstance.HitStarted(attackType);
                }
            }
        }


        private void resetSwingVariables()
        {
            swingFired = false;
            hasHit = false;
            hitFired = false;
            possibleStab = false;
            isSwinging = false;
        }

        private void SetDelay(float timeToDelay)
        {
            delayLength = timeToDelay;
            delayStartTime = Time.time;
            delayAttack = true;
            CameraHandler.SetFreezeMovement();
        }

    }
}
