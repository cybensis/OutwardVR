using UnityEngine;

namespace OutwardVR.body
{
    public class FixHeadRotation : MonoBehaviour
    {

        private GameObject character;
        void Start()
        {
            character = Camera.main.transform.root.gameObject;
        }

        void LateUpdate()
        {
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.Rotate(0, character.transform.eulerAngles.y, 0);
        }

    }
}
