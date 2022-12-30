using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using NRKernal;

public class TrackingItemsController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public NRPointerRaycaster _Raycaster;
    public GrabbableController _GrabbableItemsController;
    public CornerObjController _CornerObjController;
    public GameObject DisplayItemsLayer;
    public float InitializingDuration = 0;

    private bool isNavigating = false;
    private bool isPointerEnter = false;
    private bool canDetectRaycast = false;
    private GameObject NearestObject = null;
    private Vector3[] ObjectPositionArray = null;

    void Start()
    {
        int objectCount = DisplayItemsLayer.transform.childCount;
        ObjectPositionArray = new Vector3[objectCount];

        // 初始化展示节点中所有组件的位置，方便后续计算最近距离物体
        for (int i = 0; i < objectCount; i++)
        {
            GameObject child = DisplayItemsLayer.transform.GetChild(i).gameObject;

            ObjectPositionArray[i] = transform.InverseTransformPoint(child.transform.position);
        }

        StopTracking();
        
    }

    void Update()
    {
        if(isPointerEnter == true && ObjectPositionArray.Length > 0 && canDetectRaycast)
        {
            UpdateNearestObject();
        }
    }

    public void FinishInitializing()
    {
        StartRaycastDetection();
    }

    public void TargetFound()
    {
        _CornerObjController.ActiveCornerObjects();
    }

    public void TargetLost()
    {
        _CornerObjController.InactiveCornerObjects();
    }

    public void StartTracking()
    {
        DisplayItemsLayer.SetActive(true);

        isNavigating = true;
    }

    public void StopTracking()
    {
        DisplayItemsLayer.SetActive(false);

        isNavigating = false;
    }

    public void StartRaycastDetection()
    {
        canDetectRaycast = true;
    }

    public void StopRayastDetection()
    {
        canDetectRaycast = false;

        ResetAll();
    }

    private void UpdateNearestObject()
    {
        GameObject currNearestObject = FindNearestObject();

        if (NearestObject != currNearestObject)
        {
            if (NearestObject != null) {
                RestoreNearestObject();
            }

            NearestObject = currNearestObject; // 实时计算“最近物体”，并更新样式
            ActiveNearestObject();
        }
    }

    public void RestoreObject(string name)
    {
        Debug.Log("restore object: " + name);

        DisplayItemsLayer.transform.Find(name).gameObject.SetActive(true);
    }

    private void RestoreNearestObject()
    {
        NearestObject.GetComponent<DisplayObjectController>().ChangeToDefaultState();

        ResetAll();
    }

    private void ActiveNearestObject()
    {
        if(NearestObject == null)
        {
            return;
        }

        NearestObject.GetComponent<DisplayObjectController>().ChangeToHoverState();
    }

    public void ResetAll()
    {
        foreach(Transform child in DisplayItemsLayer.transform)
        {
            child.GetComponent<DisplayObjectController>().ChangeToDefaultState();
        }

        NearestObject = null;
    }

    private GameObject FindNearestObject()
    {
        RaycastResult firstHit = _Raycaster.FirstRaycastResult();
        Vector3 hitPointPosition = transform.InverseTransformPoint(firstHit.worldPosition);
        float nearsetDistance = float.MaxValue;
        GameObject currNearestObject = null;

        for (int i = 0; i < ObjectPositionArray.Length; i++)
        {
            if(DisplayItemsLayer.transform.GetChild(i).gameObject.activeSelf == false)
            {
                continue;
            }

            float distance = Vector3.Distance(hitPointPosition, ObjectPositionArray[i]);

            if (nearsetDistance > distance)
            {
                // 如果当前物体距离hitpoint更近，则当前物体为新的NearestObject
                currNearestObject = DisplayItemsLayer.transform.GetChild(i).gameObject;
                nearsetDistance = distance;
            }
        }

        return currNearestObject;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isNavigating == false || canDetectRaycast == false || NearestObject == null)
        {
            Debug.Log("[Player] No visuable object to active && Stop detecting raycast");

            return;
        }

        Debug.Log("[Player] Pointer click, the nearest object is: " + NearestObject.name);

        _GrabbableItemsController.ActiveGrabbableItem(NearestObject);

        NearestObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(isNavigating == false || canDetectRaycast == false)
        {
            return;
        }

        Debug.Log("[Player] Pointer enter.");

        isPointerEnter = true;

        //NRInput.ReticleVisualActive = false;
        NRInput.LaserVisualActive = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isNavigating == false || canDetectRaycast == false)
        {
            return;
        }

        Debug.Log("[Player] Pointer exit.");

        isPointerEnter = false;

        //NRInput.ReticleVisualActive = true;
        NRInput.LaserVisualActive = true;

        ResetAll();
    }
}
