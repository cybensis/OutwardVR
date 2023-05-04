using UnityEngine;
using Valve.VR;

namespace OutwardVR.body
{
    public class ArmIK : MonoBehaviour
    {

        /// <summary>
        /// Chain length of bones
        /// </summary>
        public int ChainLength = 2;

        /// <summary>
        /// Target the chain should bent to
        /// </summary>
        public Transform Target;
        public Transform Pole;

        /// <summary>
        /// Solver iterations per update
        /// </summary>
        [Header("Solver Parameters")]
        public int Iterations = 10;

        /// <summary>
        /// Distance when the solver stops
        /// </summary>
        public float Delta = 0.001f;

        /// <summary>
        /// Strength of going back to the start position.
        /// </summary>
        [Range(0, 1)]
        public float SnapBackStrength = 1f;


        protected float[] BonesLength; //Target to Origin
        protected float CompleteLength;
        protected Transform[] Bones;
        protected Vector3[] Positions;
        protected Vector3[] StartDirectionSucc;
        protected Quaternion[] StartRotationBone;
        protected Quaternion StartRotationTarget;
        protected Transform Root;
        private GameObject[] fingers;
        private float x = 0;
        private float y = 0;
        private float z = 0;

        private Character character;

        // Start is called before the first frame update
        void Awake()
        {
            Init();
        }

        void Init()
        {
            //initial array
            Bones = new Transform[ChainLength + 1];
            fingers = new GameObject[5];
            Positions = new Vector3[ChainLength + 1];
            BonesLength = new float[ChainLength];
            StartDirectionSucc = new Vector3[ChainLength + 1];
            StartRotationBone = new Quaternion[ChainLength + 1];
            character = transform.root.GetComponent<Character>();
            //find root
            Root = transform;
            for (var i = 0; i <= ChainLength; i++)
            {
                if (Root == null)
                    throw new UnityException("The chain value is longer than the ancestor chain!");
                Root = Root.parent;
            }

            //init target
            if (Target == null)
            {
                if (name == "hand_left")
                {
                    Target = CameraManager.LeftHand.transform;
                    if (Pole == null)
                        Pole = new GameObject("LeftArmPole").transform;
                    fingers[0] = transform.GetChild(6).gameObject;
                    fingers[1] = transform.GetChild(1).gameObject;
                    fingers[2] = transform.GetChild(2).gameObject;
                    fingers[3] = transform.GetChild(3).gameObject;
                    fingers[4] = transform.GetChild(4).gameObject;
                }
                else
                {
                    Target = CameraManager.RightHand.transform;
                    if (Pole == null)
                        Pole = new GameObject("RightArmPole").transform;
                    fingers[0] = transform.GetChild(5).gameObject;
                    fingers[1] = transform.GetChild(0).gameObject;
                    fingers[2] = transform.GetChild(1).gameObject;
                    fingers[3] = transform.GetChild(2).gameObject;
                    fingers[4] = transform.GetChild(3).gameObject;
                }
                DontDestroyOnLoad(Pole);
                SetPositionRootSpace(Target, GetPositionRootSpace(transform));
            }


            //init data
            var current = transform;
            CompleteLength = 0;
            for (var i = Bones.Length - 1; i >= 0; i--)
            {
                Bones[i] = current;
                StartRotationBone[i] = GetRotationRootSpace(current);

                if (i == Bones.Length - 1)
                {
                    //leaf
                    StartDirectionSucc[i] = GetPositionRootSpace(Target) - GetPositionRootSpace(current);
                }
                else
                {
                    //mid bone
                    StartDirectionSucc[i] = GetPositionRootSpace(Bones[i + 1]) - GetPositionRootSpace(current);
                    BonesLength[i] = StartDirectionSucc[i].magnitude;
                    CompleteLength += BonesLength[i];
                }
                if (name == "hand_right")
                {
                    BonesLength[0] += 0.15f;
                    CompleteLength += 0.15f;
                }
                current = current.parent;
            }



        }


        private float delayLength = 0f;
        private bool delayTracking = false;
        private float delayStartTime;

        public void SetDelay(float timeToDelay)
        {
            delayLength = timeToDelay;
            delayStartTime = Time.time;
            delayTracking = true;
        }

        public bool isDelayed()
        {
            return delayTracking;
        }

        void LateUpdate()
        {
            //if (delayTracking && Time.time - delayStartTime <= delayLength)
            //    return;
            //else if (delayTracking && Time.time - delayStartTime > delayLength)
            //    delayTracking = false;
            ResolveIK();
        }

        private void ResolveIK()
        {
            if (Target == null)
            {
                Init();
                Logs.WriteWarning(name + " target is null");
            }



            if (BonesLength.Length != ChainLength)
                Init();

            // Set the pole position from the head
            if (name == "hand_left")
                Pole.position = Camera.main.transform.parent.parent.parent.position + Camera.main.transform.parent.parent.parent.right * -0.75f + Camera.main.transform.parent.parent.parent.forward * 1f + Camera.main.transform.parent.parent.parent.up * -0.5f;
            else
                Pole.position = Camera.main.transform.parent.parent.parent.position + Camera.main.transform.parent.parent.parent.right * 1.5f + Camera.main.transform.parent.parent.parent.forward * 1.25f + Camera.main.transform.parent.parent.parent.up * -0.5f;

            if (name == "hand_left")
                TrackLeftHandFingers();
            else
                TrackRightHandFingers();


            //get position
            for (int i = 0; i < Bones.Length; i++)
                Positions[i] = GetPositionRootSpace(Bones[i]);

            Vector3 targetPosition = Target.position;
            // This offsets the VR hands so they align better with the in game hands
            if (name == "hand_left")
                targetPosition += Target.right * 0 + Target.up * 0.05f + Target.forward * -0.15f;
            else
                targetPosition += Target.right * 0.05f + Target.up * 0.05f + Target.forward * -0.15f;

            // Trying to figure out how to stick left hand onto two handed weapons
            if (name == "hand_left" && character.CurrentWeapon != null && character.CurrentWeapon.TwoHanded && character.CurrentWeapon.TwoHand != Equipment.TwoHandedType.DualWield && character.CurrentWeapon.Type != Weapon.WeaponType.Bow)
            {
                Target = CameraManager.RightHand.transform;
                Transform rightHand = character.CurrentWeapon.CurrentVisual.transform.parent.parent.parent;
                targetPosition = Target.position + rightHand.forward * -0.4f + rightHand.transform.up * -0.135f + rightHand.right * -0.025f;
            }
            else if (name == "hand_left" && Target.name == "RightHand")
            {
                Target = CameraManager.LeftHand.transform;
            }

            targetPosition = Quaternion.Inverse(Root.rotation) * (targetPosition - Root.position);

            //1st is possible to reach?
            if ((targetPosition - GetPositionRootSpace(Bones[0])).sqrMagnitude >= CompleteLength * CompleteLength)
            {
                //just strech it
                var direction = (targetPosition - Positions[0]).normalized;
                //set everything after root
                for (int i = 1; i < Positions.Length; i++)
                    Positions[i] = Positions[i - 1] + direction * BonesLength[i - 1];
            }
            else
            {
                for (int i = 0; i < Positions.Length - 1; i++)
                    Positions[i + 1] = Vector3.Lerp(Positions[i + 1], Positions[i] + StartDirectionSucc[i], SnapBackStrength);

                for (int iteration = 0; iteration < Iterations; iteration++)
                {
                    //back
                    for (int i = Positions.Length - 1; i > 0; i--)
                    {
                        if (i == Positions.Length - 1)
                            Positions[i] = targetPosition; //set it to target
                        else
                            Positions[i] = Positions[i + 1] + (Positions[i] - Positions[i + 1]).normalized * BonesLength[i]; //set in line on distance
                    }

                    //forward
                    for (int i = 1; i < Positions.Length; i++)
                        Positions[i] = Positions[i - 1] + (Positions[i] - Positions[i - 1]).normalized * BonesLength[i - 1];

                    //close enough?
                    if ((Positions[Positions.Length - 1] - targetPosition).sqrMagnitude < Delta * Delta)
                        break;
                }
            }

            //move towards pole
            if (Pole != null)
            {
                var polePosition = GetPositionRootSpace(Pole);
                for (int i = 1; i < Positions.Length - 1; i++)
                {
                    var plane = new Plane(Positions[i + 1] - Positions[i - 1], Positions[i - 1]);
                    var projectedPole = plane.ClosestPointOnPlane(polePosition);
                    var projectedBone = plane.ClosestPointOnPlane(Positions[i]);
                    var angle = Vector3.SignedAngle(projectedBone - Positions[i - 1], projectedPole - Positions[i - 1], plane.normal);
                    Positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (Positions[i] - Positions[i - 1]) + Positions[i - 1];
                }
            }

            //set position & rotation
            for (int i = 0; i < Positions.Length; i++)
            {
                if (i == Positions.Length - 1)
                {
                    Bones[i].rotation = Target.transform.rotation;
                    //Bones[i].localRotation = Target.transform.localRotation;
                    if (name == "hand_left")
                        Bones[i].Rotate(-34, 15, -217);
                    else
                        Bones[i].Rotate(-34, 15, -200);

                    //if (name == "hand_left" && character.CurrentWeapon != null && character.CurrentWeapon.TwoHanded)
                    //{
                    //    Bones[i].rotation = CameraManager.RightHand.transform.rotation;
                    //    Bones[i].Rotate(0,0,180f);

                    //}
                }
                else
                    SetRotationRootSpace(Bones[i], Quaternion.FromToRotation(StartDirectionSucc[i], Positions[i + 1] - Positions[i]) * Quaternion.Inverse(StartRotationBone[i]));
                SetPositionRootSpace(Bones[i], Positions[i]);
            }
        }

        private Vector3 GetPositionRootSpace(Transform current)
        {
            if (Root == null)
                return current.position;
            else
                return Quaternion.Inverse(Root.rotation) * (current.position - Root.position);
        }

        private void SetPositionRootSpace(Transform current, Vector3 position)
        {
            if (Root == null)
                current.position = position;
            else
                current.position = Root.rotation * position + Root.position;
        }

        private Quaternion GetRotationRootSpace(Transform current)
        {
            //inverse(after) * before => rot: before -> after
            if (Root == null)
                return current.rotation;
            else
                return Quaternion.Inverse(current.rotation) * Root.rotation;
        }

        private void SetRotationRootSpace(Transform current, Quaternion rotation)
        {
            if (Root == null)
                current.rotation = rotation;
            else
                current.rotation = Root.rotation * rotation;
        }


        private void TrackLeftHandFingers()
        {
            // Set thumb position
            fingers[0].transform.localRotation = Quaternion.identity;
            fingers[0].transform.Rotate(10, 90 - 80 * SteamVR_Actions._default.SkeletonLeftHand.fingerCurls[0], 40);
            fingers[0].transform.GetChild(0).GetChild(0).localRotation = Quaternion.identity;
            fingers[0].transform.GetChild(0).GetChild(0).Rotate(360 - 70 * SteamVR_Actions._default.SkeletonLeftHand.fingerCurls[0], 360 - 30 * SteamVR_Actions._default.SkeletonLeftHand.fingerCurls[0], 0);
            for (int i = 1; i < 5; i++)
            {
                // X bends fingers backwards and forwards
                // Y twists them around
                // X is left and right

                fingers[i].transform.localRotation = Quaternion.identity;
                // Set the 4 fingers rotation to straight, then bend based on finger curl input
                fingers[i].transform.Rotate(200 + 50 * SteamVR_Actions._default.SkeletonLeftHand.fingerCurls[i], 250 - 10 * SteamVR_Actions._default.SkeletonLeftHand.fingerCurls[i], 180 + 10 * SteamVR_Actions._default.SkeletonLeftHand.fingerCurls[i]);

                fingers[i].transform.GetChild(0).localRotation = Quaternion.identity;
                // Pointer and middle finger have an extra joint so 
                if (i == 1 || i == 2)
                {
                    fingers[i].transform.GetChild(0).Rotate(350 - 55 * SteamVR_Actions._default.SkeletonLeftHand.fingerCurls[i], 0, 0);
                    fingers[i].transform.GetChild(0).GetChild(0).localRotation = Quaternion.identity;
                    fingers[i].transform.GetChild(0).GetChild(0).Rotate(350 - 55 * SteamVR_Actions._default.SkeletonLeftHand.fingerCurls[i], 0, 0);
                }
                else
                    fingers[i].transform.GetChild(0).Rotate(350 - 70 * SteamVR_Actions._default.SkeletonLeftHand.fingerCurls[i], 0, 0);
            }
        }
        private void TrackRightHandFingers()
        {
            fingers[0].transform.localRotation = Quaternion.identity;
            fingers[0].transform.Rotate(350, 300 + 50 * SteamVR_Actions._default.SkeletonRightHand.fingerCurls[0], 320);
            fingers[0].transform.GetChild(0).GetChild(0).localRotation = Quaternion.identity;
            fingers[0].transform.GetChild(0).GetChild(0).Rotate(340 - 20 * SteamVR_Actions._default.SkeletonRightHand.fingerCurls[0], 20, 0 + 70 * SteamVR_Actions._default.SkeletonRightHand.fingerCurls[0]);

            for (int i = 1; i < 5; i++)
            {
                fingers[i].transform.localRotation = Quaternion.identity;
                fingers[i].transform.Rotate(350 - 50 * SteamVR_Actions._default.SkeletonRightHand.fingerCurls[i], 275 + 10 * SteamVR_Actions._default.SkeletonRightHand.fingerCurls[i], 0);
                fingers[i].transform.GetChild(0).localRotation = Quaternion.identity;

                if (i == 1 || i == 2)
                {
                    fingers[i].transform.GetChild(0).Rotate(350 - 50 * SteamVR_Actions._default.SkeletonRightHand.fingerCurls[i], 0, 0);
                    fingers[i].transform.GetChild(0).GetChild(0).localRotation = Quaternion.identity;
                    fingers[i].transform.GetChild(0).GetChild(0).Rotate(350 - 50 * SteamVR_Actions._default.SkeletonRightHand.fingerCurls[i], 0, 0);

                }
                else
                    fingers[i].transform.GetChild(0).Rotate(350 - 80 * SteamVR_Actions._default.SkeletonRightHand.fingerCurls[i], 0, 0);
            }
        }

        //void OnCollisionEnter(Collision collision)
        //{
        //    Logs.WriteWarning(collision.gameObject.name + " " + Time.time);
        //}

        //void OnTriggerEnter(Collider other)
        //{
        //    Logs.WriteWarning(other.gameObject.name + " " + Time.time);
        //}


        //private int chainLength = 2;
        //private Transform target;
        //private int iterations = 10;

        //private Transform pole;

        //private float delta = 0.001f;

        //private Transform[] bones;
        //private float[] bonesLength;
        //private Vector3[] positions;
        //private float snapBackStrength = 1f;
        //private Vector3[] startDirectionSucc;
        //private Quaternion[] startRotationBone;
        //private Quaternion startRotationTarget;
        //private Quaternion startRotationRoot;

        //private float completeLength;

        //void Awake() {
        //    Init();
        //}

        //void Init()
        //{
        //    bones = new Transform[chainLength + 1];
        //    bonesLength = new float[chainLength];
        //    positions = new Vector3[chainLength + 1];
        //    startDirectionSucc = new Vector3[chainLength + 1];
        //    startRotationBone = new Quaternion[chainLength + 1];

        //    completeLength = 0;
        //    if (this.name == "hand_left")
        //    {
        //        target = CameraManager.LeftHand.transform;
        //        pole = CameraManager.RightHand.transform;
        //    }
        //    else { 
        //        target = CameraManager.RightHand.transform;
        //        pole = CameraManager.LeftHand.transform;
        //    }

        //    startRotationTarget = target.rotation;

        //    Transform currentTransform = transform;
        //    // Following a tutorial for the IK, not sure why the for loop starts like this just yet
        //    for (int i = bones.Length - 1; i >= 0; i--) {
        //        bones[i] = currentTransform;

        //        // i == bones.length-1 is a leaf bone (last bone) that should be attached to the target (hand I think?) and we don't want the length there,
        //        if (i == bones.Length - 1)
        //            startDirectionSucc[i] = target.position - currentTransform.position;
        //        // i < bones.length -1  is a mid bone which we want the length for
        //        if (i < bones.Length - 1) {
        //            startDirectionSucc[i] = bones[i + 1].position - currentTransform.position;
        //            bonesLength[i] = (bones[i + 1].position - currentTransform.position).magnitude;
        //            completeLength += bonesLength[i];
        //        } 

        //        currentTransform = currentTransform.parent;
        //    }

        //}


        //void LateUpdate() {
        //    ResolveIK();
        //}

        //private void ResolveIK()
        //{
        //    if (target == null)
        //        return;

        //    // If the chain length does not fit to the bone length for whatever reason, init it again
        //    if (bonesLength.Length != chainLength)
        //        Init();

        //    // Get positions
        //    for (int i = 0; i < bones.Length; i++) {
        //        positions[i] = bones[i].position;
        //    }

        //    Quaternion rootRot = (bones[0].parent != null) ? bones[0].parent.rotation : Quaternion.identity;
        //    Quaternion rootRotDiff = rootRot * Quaternion.Inverse(startRotationRoot);


        //    // 1st is possible to reach?
        //    if ((target.position - bones[0].position).sqrMagnitude >= completeLength * completeLength)
        //    {
        //        // Get direction by subtracting the shoulder pos from the hand target pos and normalizing it
        //        Vector3 direction = (target.position - positions[0]).normalized;

        //        for (int i = 1; i < positions.Length; i++)
        //            // Set the position of the bone to the position of its predecessor plus the direction * the bone length of the predecessor
        //            positions[i] = positions[i - 1] + direction * bonesLength[i - 1];
        //    }
        //    else {

        //        for (int i = 0; i < positions.Length - 1; i++)
        //            positions[i + 1] = Vector3.Lerp(positions[i + 1], positions[i] + rootRotDiff * startDirectionSucc[i], snapBackStrength);

        //        for (int i = 0; i < iterations; i++) {
        //            for (int a = positions.Length - 1; a > 0; a--) {
        //                // moving backwards
        //                if (a == positions.Length - 1)
        //                    // set that hand position to the target position
        //                    positions[a] = target.position;
        //                else
        //                    // I think this line makes it so the distance of a joint from another joint is always at the length of the bone
        //                    positions[a] = positions[a + 1] + (positions[a] - positions[a + 1]).normalized * bonesLength[a];

        //            }
        //            // moving forward
        //            for (int a = 1; a < positions.Length; a++) {
        //                // same as the part above but in reverse
        //                positions[a] = positions[a - 1] + (positions[a] - positions[a - 1]).normalized * bonesLength[a - 1];       
        //            }
        //            if ((positions[positions.Length - 1] - target.position).sqrMagnitude < delta * delta)
        //                break;
        //        }
        //    }
        //    // move towards pole
        //    if (pole != null)
        //    {
        //        for (int i = 1; i < positions.Length - 1; i++) {
        //            Plane plane = new Plane(positions[i + 1] - positions[i - 1], positions[ i - 1]);
        //            Vector3 projectedPole = plane.ClosestPointOnPlane(pole.position);
        //            Vector3 projectedBone = plane.ClosestPointOnPlane(positions[i]);
        //            float angle = Vector3.SignedAngle(projectedBone - positions[i - 1], projectedPole - positions[i - 1], plane.normal);
        //            positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (positions[i] - positions[i - 1]) + positions[i - 1];
        //        }
        //    }

        //    // set the bones positions after finishing calculations
        //    for (int i = 0; i < positions.Length; i++)
        //    {
        //        if (i == positions.Length - 1)
        //            bones[i].rotation = target.rotation * Quaternion.Inverse(startRotationTarget) * startRotationBone[i];
        //        else
        //            bones[i].rotation = Quaternion.FromToRotation(startDirectionSucc[i], positions[i + 1] - positions[i]) * startRotationBone[i];
        //        bones[i].position = positions[i];
        //    }

        //}

    }
}