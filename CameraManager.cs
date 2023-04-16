
using OutwardVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.Extras;

public class SceneHandler : MonoBehaviour
{
    public SteamVR_LaserPointer laserPointer;
    public GraphicRaycaster m_Raycaster;
    void Awake()
    {
        m_Raycaster = GetComponent<GraphicRaycaster>();
        //laserPointer = gameObject.GetComponent<SteamVR_LaserPointer>();
        //laserPointer.PointerIn += PointerInside;
        //laserPointer.PointerOut += PointerOutside;
        //laserPointer.PointerClick += PointerClick;
    }


    private void Update() {
        //interact.Process();

        PointerEventData pointerData = new PointerEventData(EventSystem.current);

        //pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        //EventSystem.current.RaycastAll(pointerData, results);

        m_Raycaster.Raycast(pointerData, results);

        if (results.Count > 0)
        {
            Logs.WriteWarning(results[0].gameObject.name);
        }

        //Ray raycast = new Ray(transform.position, transform.forward);
        //RaycastHit hit;
        //bool bHit = Physics.Raycast(raycast, out hit);

        //PointerEventData pointerData = new PointerEventData(EventSystem.current);

        //pointerData.position = transform.position;
        //List<RaycastResult> results = new List<RaycastResult>();
        //EventSystem.current.RaycastAll(pointerData, results);
        //Logs.WriteWarning(results.Count);
        //if (results.Count > 0)
        //{
        //    Logs.WriteWarning(results[0].gameObject.name);
        //}

        //if (bHit)
        //{
        //    Logs.WriteWarning(hit.transform.parent.name);

        //}
    }


    public void PointerClick(object sender, PointerEventArgs e)
    {
        if (e.target.name == "Cube")
        {
            Logs.WriteWarning("Cube was clicked");
        }
        else if (e.target.name == "Button")
        {
            Logs.WriteWarning("Button was clicked");
        }
    }

    public void PointerInside(object sender, PointerEventArgs e)
    {
        if (e.target.name == "Cube")
        {
            Logs.WriteWarning("Cube was entered");
        }
        else if (e.target.name == "Button")
        {
            Logs.WriteWarning("Button was entered");
        }
    }

    public void PointerOutside(object sender, PointerEventArgs e)
    {
        if (e.target.name == "Cube")
        {
            Logs.WriteWarning("Cube was exited");
        }
        else if (e.target.name == "Button")
        {
            Logs.WriteWarning("Button was exited");
        }
    }
}

//using System;
//using System.Collections.Generic;
//using System.Diagnostics.PerformanceData;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using Valve.VR;

//namespace OutwardVR
//{
//    public class LaserInput : BaseInputModule
//    {
//        //public static GameObject currentObject;
//        //int currentID;
//        private const float rayDistance = 30f;
//        public Camera EventCamera;
//        private Vector3 lastHeadPose;
//        private PointerEventData pointerData;

//        //void Start() {
//        //    currentObject = null;
//        //    currentID = 0;  
//        //}


//        public override void Process() {
//            //RaycastHit hit;
//            var isHit = Physics.Raycast( transform.position, transform.forward, out var hit, rayDistance);
//            //var pointerPosition = EventCamera.WorldToScreenPoint(hit.point);
//            //if (hit.collider != null && hit.collider.gameObject != null) { 
//            //    Logs.WriteWarning(hit.collider.gameObject.name);
//            //}


//            //if (pointerData == null)
//            //{
//            //    pointerData = new PointerEventData(eventSystem);
//            //    lastHeadPose = pointerPosition;
//            //}

//            //// Cast a ray into the scene
//            //pointerData.Reset();
//            //pointerData.position = pointerPosition;
//            //eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
//            //pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
//            //m_RaycastResultCache.Clear();
//            //pointerData.delta = pointerPosition - lastHeadPose;
//            //lastHeadPose = hit.point;
//        }

//        //void Update()
//        //{




//        //    try
//        //    {
//        //        RaycastHit[] hits;
//        //        hits = Physics.RaycastAll(transform.position, transform.forward, 100.0f);
//        //        Logs.WriteWarning(hits);
//        //        for (int i = 0; i < hits.Length; i++)
//        //        {
//        //            RaycastHit hit = hits[i];
//        //            Logs.WriteWarning(hit);

//        //            if (hit.collider == null || hit.collider.gameObject == null)
//        //            {
//        //                continue;
//        //            }
//        //            else
//        //            {
//        //                int id = hit.collider.gameObject.GetInstanceID();

//        //                if (currentID != id)
//        //                {
//        //                    currentID = id;
//        //                    Logs.WriteWarning(id);
//        //                    currentObject = hit.collider.gameObject;
//        //                    Logs.WriteWarning(currentObject);


//        //                    string name = currentObject.name;

//        //                    if (name == "Next")
//        //                    {
//        //                        Logs.WriteWarning("HIT NEXT");
//        //                    }

//        //                    string tag = currentObject.tag;
//        //                    if (tag == "Button")
//        //                    {
//        //                        Logs.WriteWarning("HIT BUTTON");

//        //                    }
//        //                }
//        //            }
//        //        }
//        //    }
//        //    catch (Exception e)
//        //    {
//        //        //Logs.WriteInfo(e);
//        //    }

//        //}

//    }

//}
