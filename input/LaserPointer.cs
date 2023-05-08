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
        private float x, y, z;

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
            {
                worldItem = hit.collider.gameObject;
            }
            else
            {
                worldItem = null;
            }
                if (x == 1)
            {
                Physics.Raycast(transform.position, transform.up * -1, out hit, raycastLength, Global.WorldItemsMask);
                if (hit.collider != null)
                {
                    Logs.WriteWarning(hit.collider.gameObject.transform.parent.parent.name);
                }
            }
            if (y == 1)
            {
                Physics.Raycast(transform.position, transform.forward * -1, out hit, raycastLength, Global.WorldItemsMask);
                if (hit.collider != null)
                {
                    Logs.WriteWarning(hit.collider.gameObject.transform.parent.parent.name);
                }
            }
            if (z == 1)
            {
                Physics.Raycast(transform.position, transform.right * -1, out hit, raycastLength, Global.WorldItemsMask);
                if (hit.collider != null)
                {
                    Logs.WriteWarning(hit.collider.gameObject.transform.parent.parent.name);
                }
            }
            if (x == 2)
            {
                Physics.Raycast(transform.position, transform.up, out hit, raycastLength, Global.WorldItemsMask);
                if (hit.collider != null)
                {
                    Logs.WriteWarning(hit.collider.gameObject.transform.parent.parent.name);
                }
            }
            if (y == 2)
            {
                Physics.Raycast(transform.position, transform.forward, out hit, raycastLength, Global.WorldItemsMask);
                if (hit.collider != null)
                {
                    Logs.WriteWarning(hit.collider.gameObject.transform.parent.parent.name);
                }
            }
            if (z == 2)
            {
                Physics.Raycast(transform.position, transform.right, out hit, raycastLength, Global.WorldItemsMask);
                if (hit.collider != null)
                {
                    Logs.WriteWarning(hit.collider.gameObject.transform.parent.parent.name);
                }
            }
        }

    }
}
