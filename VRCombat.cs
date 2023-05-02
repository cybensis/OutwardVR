using MapMagic;
using OutwardVR.combat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;
using static FightRecap;

namespace OutwardVR
{
    public class VRCombat : MonoBehaviour
    {

        private float x , y, z;

        private static float BASE_WEAPON_REACH = 4.8f;
        private static float BASE_RAYCAST_LENGTH = 0.75f;


        // To be registered as swinging, the swing needs to reach a velocity of 1.35 which is kind of slow
        // but players shouldn't have to swing fast to actualy swing (going on saints and sinners logic)
        private const float VELOCITY_THRESHOLD = 1.35f;
        // A swing at the threshold velocity needs to be maintained for 0.35f seconds for it to be a full swing
        private const float SWING_MAINTAINED_THRESHOLD = 0.35f;
        // No matter how fast the player swings, it must be maintained for atleast 0.08 seconds
        private const float SWING_MAINTAINED_MIN = 0.08f;
        // Threshold - min is what this value represents, this is the time difference allowed to be modified by reaching a velocity beyond threshold
        private const float SWING_VELOCITY_TIME_MODIFIER = SWING_MAINTAINED_THRESHOLD - SWING_MAINTAINED_MIN;
        // A velocity of 6 is pretty fast, players really shouldn't go beyond this
        private const float MAX_VELOCITY = 6f;
        // Used in the swing calculation where we divide this value by the current velocity minus the threshold velocity
        private const float MAX_VELOCITY_MINUS_THRESHOLD = 4.65f;


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
        private float lastHitTime = 0f;
        private bool isBlocking = false;
        RaycastHit hit;

        private float raycastLength = 0f;

        Character characterInstance;
        private ArmIK handIK;

        // When the sword is held to the side to block, it should be within range of 1.0 to swordBlockRangeTolerance otherwise its not gunna count as a block
        private float swordBlockRangeTolerance = 0.875f;

        private float attackDelay = 0.7f;


        private float delayLength = 0f;
        private bool delayAttack = false;
        private float delayStartTime;


        private void InitVars() {
            handIK = base.transform.parent.parent.parent.GetComponent<ArmIK>();
            characterInstance = Camera.main.transform.root.GetComponent<Character>();
            if (handIK.name == "hand_left" && characterInstance.LeftHandWeapon.CurrentVisual.name != base.name)
                return;
            else if (handIK.name == "hand_right" && characterInstance.CurrentWeapon.CurrentVisual.name != base.name)
                return;
            MeleeWeapon weapon = GetComponent<ItemVisual>().m_item as MeleeWeapon;
            // The two BASE vars were made based on the iron swords reach, so by dividing the reach of the current weapon by the iron swords reach, 
            // we get a modifier value to multiply the raycast length by.
            raycastLength = (weapon.Reach / BASE_WEAPON_REACH) * BASE_RAYCAST_LENGTH;
            // Weapon Types:
            //Sword_1H = 0,
            //Axe_1H = 1,
            //Mace_1H = 2,
            //Dagger_OH = 30,
            //Chakram_OH = 40,
            //Pistol_OH = 45,
            //Halberd_2H = 50,
            //Sword_2H = 51,
            //Axe_2H = 52,
            //Mace_2H = 53,
            //Spear_2H = 54,
            //FistW_2H = 55,
            //Shield = 100,
            //Arrow = 150,
            //Bow = 200
            if (weapon.Type == Weapon.WeaponType.Halberd_2H ||
                weapon.Type == Weapon.WeaponType.Sword_2H ||
                weapon.Type == Weapon.WeaponType.Axe_2H ||
                weapon.Type == Weapon.WeaponType.Mace_2H ||
                weapon.Type == Weapon.WeaponType.Spear_2H
            )
                attackDelay = 0.9f;
            else if (weapon.Type == Weapon.WeaponType.Dagger_OH)
                attackDelay = 0.2f;
            if (weapon.Type == Weapon.WeaponType.Halberd_2H)
                raycastLength += 0.1f;
        }


        void Update()
        {
            if (handIK == null || characterInstance == null || raycastLength == 0f)
                InitVars();
            if (delayAttack || !NetworkLevelLoader.Instance.IsOverallLoadingDone || !NetworkLevelLoader.Instance.AllPlayerReadyToContinue)
                return;
            if (handIK.name == "hand_right") { 
                if (characterInstance.CurrentWeapon.TwoHanded)
                    base.transform.parent.localPosition = new Vector3(0f, -0.6f, 0f);
                else
                    base.transform.parent.localPosition = Vector3.zero;
            }


            float swingVelocity;
            if (handIK.name == "hand_right")
                swingVelocity = Mathf.Clamp(SteamVR_Actions._default.SkeletonRightHand.velocity.magnitude, 0, MAX_VELOCITY);
            else
                swingVelocity = Mathf.Clamp(SteamVR_Actions._default.SkeletonLeftHand.velocity.magnitude, 0, MAX_VELOCITY);
            // Using the constants explained above, we check if the time swung for is greater than or equal to the threshold swing time, with a modified value for extended beyond the threshold velocity
            // The max velocitiy minus the threshold value, 4.65f, divided by the swingvelocity capped at 6f minus the threshold velocity will return a number less than or equal to 1, then this is used
            // by multiplying it against the time modifier which is then substracted from the maintained swing time threshold. E.g. if we have a swing of velocity 6, 4.65 / (6 - 1.35) will be 1, then 1 * 0.23 
            // will obviously be 0.23, then the maintained swing time threshold value minus 0.23 is 0.12 which is the minimum time we allow for something to count as a swing even at the max velocity.
            if (isSwinging && !swingFired && Time.time - swingStart >= SWING_MAINTAINED_THRESHOLD - (SWING_VELOCITY_TIME_MODIFIER * ((swingVelocity - VELOCITY_THRESHOLD) / MAX_VELOCITY_MINUS_THRESHOLD)))
            {
                swingFired = true;
                if (characterInstance != null && characterInstance.HasEnoughStamina(characterInstance.CurrentWeapon.StamCost))
                {
                    // Figure out what the attack ID's for different combos are and for heavy attacks then manually set these values here or something
                    characterInstance.AttackInput(characterInstance.m_nextAttackType, characterInstance.m_nextAttackID);
                    //characterInstance.StartAttack(characterInstance.m_nextAttackType, characterInstance.m_nextAttackID);
                    characterInstance.HitStarted(characterInstance.m_nextAttackID);
                }
            }

            //if (isSwinging && !hasHit && Physics.Raycast(transform.parent.position, transform.right * -1, out hit, raycastLength, LayerMask.GetMask("Hitbox")) && !hit.collider.GetComponent<Hitbox>().m_ownerChar.IsLocalPlayer)
            if (isSwinging && !hasHit && Physics.Raycast(handIK.transform.position, transform.right * -1, out hit, raycastLength, LayerMask.GetMask("Hitbox")) && !hit.collider.GetComponent<Hitbox>().m_ownerChar.IsLocalPlayer)
            hasHit = true;



            //Don't want blocking to activate mid swing but want to unblock if they try to come out of a block swinging
            if (swingVelocity < VELOCITY_THRESHOLD)
            {
                // This checks if the swords right is pointed in the same direction as the bodies right, if its == 1 then its pointing exactly the same direction
                // but we want some leeway so the sword doesn't have to be held exactly at the bodies right.
                if (Vector3.Dot(transform.right, characterInstance.transform.right) >= 0.9f)
                {
                    // BlockInput is called with the argument _active (blocking is or isn't active), which calls on SendBlocKStateTrivial and if _active is true
                    // it calls on StartBlocking and that sets blocking as true, otherwise it calls StopBlocking which sets blocking to false.
                    characterInstance.BlockInput(true);
                    isBlocking = true;
                }
                else if (isBlocking && Mathf.Abs(Vector3.Dot(transform.right, characterInstance.transform.right)) < swordBlockRangeTolerance)
                {
                    // Unblock here
                    characterInstance.BlockInput(false);
                    isBlocking = false;
                    // Change this so there is only a delay if the player gets hit while blocking
                    if (OutwardVR.combat.MeleeWeapon.hitWhileBlocking)
                    {
                        SetDelay(0.7f);
                        Logs.WriteWarning("Delay after block");
                        OutwardVR.combat.MeleeWeapon.hitWhileBlocking = false;
                    }
                }
            }
            else if (swingVelocity >= VELOCITY_THRESHOLD && isBlocking) {
                if (OutwardVR.combat.MeleeWeapon.hitWhileBlocking)
                {
                    SetDelay(0.7f);
                    Logs.WriteWarning("Delay after block");
                    OutwardVR.combat.MeleeWeapon.hitWhileBlocking = false;
                }
            }


      
            // TIMING STUFF TO ADD:
            // Look at Character.UpdateWeapon for times
            // - Two handed weapons are a 0.9 second delay
            // - I think normal one handed weapons are 0.7
            // - Daggers appear to be 0.2f
            // - Something to do with the shield is 0.5 (maybe you can bash or something with it)
            // - After returning from blocking there is a 0.7 second delay
        

            if (swingFired && hasHit && !hitFired) {
                if (characterInstance.HasEnoughStamina(characterInstance.CurrentWeapon.StamCost)) {
                    hitFired = true;
                    // Try and add direction here cos I think it'll make enemies bodies ragdoll in that direction if they die
                    characterInstance.CurrentWeapon.HasHit(hit, new Vector3(0, 0, 0));
                }
            }
            

            if (swingVelocity >= VELOCITY_THRESHOLD && !isSwinging)
            {
                swingStart = Time.time;
                isSwinging = true;
                resetSwingVariables();
                // When a player starts to swing notifiy enemies as early as possible so they can dodge. This is a limitation of the VR mod since
                // the normal time to attack is pretty slow so AI migh never actually be able to dodge attacks due to the speed of VR combat.
                characterInstance.NotifyNearbyAIOfAttack();
            }
            else if (swingVelocity < VELOCITY_THRESHOLD && isSwinging)
            {
                if (swingFired) { 
                    SetDelay(attackDelay);
                    Logs.WriteWarning("Delay after swing");
                }
                isSwinging = false;
                resetSwingVariables();
            }
        }


        private void resetSwingVariables() {
            swingFired = false;
            hasHit = false;
            hitFired = false;
        }

        public void SetDelay(float timeToDelay)
        {
            delayLength = timeToDelay;
            delayStartTime = Time.time;
            delayAttack = true;
        }


        //void LateUpdate()
        //{
        //    if (name == "neck")
        //    {
        //        transform.localRotation = Quaternion.identity;
        //        transform.localPosition = Vector3.zero;
        //        transform.rotation = Quaternion.identity;
        //        transform.Rotate(x, y, z);
        //    }

        //    RaycastHit hit;
        //    //Vector3 vec = transform.forward;
        //    //if (z == 0)
        //    //    vec = transform.right;
        //    //if (y == 0)
        //    //    vec = transform.up;

        //    //if (invert)
        //    //    vec = vec * -1;

        //    if (Physics.Raycast(transform.parent.position, transform.up * -1, out hit, 0.75f, LayerMask.GetMask("Hitbox")))
        //    {
        //        if (hit.collider.gameObject != null)
        //            Logs.WriteWarning(Time.time + " " + hit.collider.gameObject);
        //    }
        //}

        //void OnCollisionEnter(Collision collision)
        //{
        //    Logs.WriteWarning("COLENTER " + collision.gameObject.name + " " + Time.time);
        //}

        void OnTriggerEnter(Collider other)
        {
            Logs.WriteWarning("TRIGENT " + other.gameObject.name + " " + Time.time);
        
            // Attacking process goes from
            // Character.UpdateWeapon() triggers StartAttack or it calls AttackInput which is for some other attacks, maybe charge
            //      StartAttack starts by calling ActionPerformed which is what stops the player from moving and doing some other stuff, and then
            //      does things like check if you have enough stamina and removes stamina if you do, and also sets a Visuals.UnarmedHitDetector which im not sure what it does.
            //      Upon determining attack type and checking a whole bunch of variables, it calls SendPerformAttack...
            //              SendPerformAttack sets the hit transform with a line cast, and sets some animation stuff and notifies AI of the attack
            //                  Not sure much of what happens inbetween, but Weapon calls HasHit which I think does the brunt of the damage stuff

            // Call StartAttack with the m_nextAttackType and m_nextAttackID, I think these should both be handled without me needing to do much. I don't think I need to do much in StartAttack ATM
            // In SendPerformAttackTrivial, I probably should stop all the animator stuff, besides that I don't think I need to do much

            // Maybe swap the collider stuff for a raycast, that way I can just use the HasHit without much messing around

            // Character.HitStarted does the linecast and calls HasHit
        }


    }
}
