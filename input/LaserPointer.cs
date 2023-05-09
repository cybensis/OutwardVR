using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;

namespace OutwardVR.input
{

    internal class LaserPointer : MonoBehaviour
    {
        private RaycastHit hit;
        public GameObject worldItem = null;
        private float raycastLength = 1.5f;

        private void Awake()
        {
            GameObject laserHolder = transform.GetComponent<SteamVR_LaserPointer>().holder;
            if (laserHolder != null)
                laserHolder.transform.Rotate(270, 0, 0);
        }

        private void Update()
        {
            Physics.Raycast(transform.position, transform.up, out hit, raycastLength, Global.WorldItemsMask);
            if (hit.collider != null)
                worldItem = hit.collider.gameObject;
            else
                worldItem = null;
        }

    }
}
