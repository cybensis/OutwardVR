using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Valve.VR;

namespace OutwardVR
{
    public class CollisionTest : MonoBehaviour
    {

        private int x , y, z;


        //void LateUpdate()
        //{
        //    transform.parent.localRotation = Quaternion.identity;
        //    transform.parent.Rotate(x,y,z);

        //}

            void OnCollisionEnter(Collision collision)
        {
            Logs.WriteWarning("COLENTER " + collision.gameObject.name + " " + Time.time);
        }

        void OnTriggerEnter(Collider other)
        {
            Logs.WriteWarning("TRIGENT " + other.gameObject.name + " " + Time.time);
        }


    }
}
