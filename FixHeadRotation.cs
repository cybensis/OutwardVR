using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;
using Valve.VR.Extras;

namespace OutwardVR
{
    public class FixHeadRotation : MonoBehaviour
    {

        private GameObject character;


        void Start() {
            character = Camera.main.transform.root.gameObject;
        }

        void LateUpdate()
        {
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.Rotate(0, character.transform.eulerAngles.y, 0);
        }


        //public override void Process()
        //{
        //    //Logs.WriteWarning("T");
        //    m_Data.Reset();
        //    m_Data.position = new Vector2(m_Camera.pixelWidth / 2, m_Camera.pixelHeight / 2);

        //    eventSystem.RaycastAll(m_Data, m_RaycastResultCache);
        //    m_Data.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
        //    if (m_Data.pointerCurrentRaycast.gameObject != null)
        //    {
        //        m_CurrentObject = m_Data.pointerCurrentRaycast.gameObject;
        //        Logs.WriteWarning(m_CurrentObject.gameObject.name);
        //        m_RaycastResultCache.Clear();

        //        HandlePointerExitAndEnter(m_Data, m_CurrentObject);

        //        if (m_ClickAction.GetStateDown(m_TargetSource))
        //        {
        //            ProcessPress(m_Data);
        //        }
        //        if (m_ClickAction.GetStateUp(m_TargetSource))
        //        {
        //            ProcessRelease(m_Data);
        //        }
        //    }
        //}


        //private void ProcessPress(PointerEventData data)
        //{


        //}

        //private void ProcessRelease(PointerEventData data)
        //{


        //}

        //public SteamVR_LaserPointer laserPointer;
        //public bool selected;
        //// Start is called before the first frame update
        //void Start()
        //{
        //    laserPointer = CameraManager.laserPointer;
        //    laserPointer.PointerIn += PointerInside;
        //    laserPointer.PointerOut += PointerOutside;
        //    selected = false;
        //}
        //// Update is called once per frame
        //void Update()
        //{

        //}
        //public void PointerInside(object sender, PointerEventArgs e)
        //{
        //    Logs.WriteWarning("pointer is inside this object" + e.target.name);
        //    if (e.target.name == this.gameObject.name && selected == false)
        //    {
        //        selected = true;
        //        Logs.WriteWarning("pointer is inside this object" + e.target.name);
        //    }
        //}
        //public void PointerOutside(object sender, PointerEventArgs e)
        //{

        //    if (e.target.name == this.gameObject.name && selected == true)
        //    {
        //        selected = false;
        //        Logs.WriteWarning("pointer is outside this object" + e.target.name);
        //    }
        //}
        //public bool get_selected_value()
        //{
        //    return selected;
        //}
    }
}
