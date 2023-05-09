using System.Runtime.CompilerServices;
using UnityEngine;

namespace OutwardVR
{
    public class Test : MonoBehaviour
    {
        private float x, y, z;
        private void LateUpdate() { 
            //transform.localPosition = new Vector3(x, y, z);
            transform.Rotate(x, y, z);
        }
    }
}
