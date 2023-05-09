using UnityEngine;

namespace OutwardVR.body
{
    public class FixHeadRotation : MonoBehaviour
    {

        private GameObject character;
        private static float x, y, z;
        void Start()
        {
            character = Camera.main.transform.root.gameObject;
        }

        void LateUpdate()
        {
            //if (transform.GetChild(3) != null) { 
            //    transform.GetChild(3).localPosition = new Vector3(x, y, z);
            //}
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.Rotate(0, character.transform.eulerAngles.y, 0);
        }

    }
}
