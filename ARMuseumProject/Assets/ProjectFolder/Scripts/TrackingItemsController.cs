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
    private bool canDetectRaycast = true;
    private bool isInitializing = true;
    private GameObject NearestObject = null;
    private Vector3 NearestObjectOriginalPosition;
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
        if(isPointerEnter && ObjectPositionArray.Length > 0 && canDetectRaycast && !isInitializing)
        {
            UpdateNearestObject();

            if(NearestObject != null)
            {
                RaycastResult firstHit = _Raycaster.FirstRaycastResult();
                Vector3 hitPointPosition = transform.InverseTransformPoint(firstHit.worldPosition);

                NearestObject.transform.localPosition = Vector3.Slerp(NearestObjectOriginalPosition, hitPointPosition, 0.1f);
            }
        }
    }

    public void FinishInitializing()
    {
        isInitializing = false;
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

            NearestObjectOriginalPosition = currNearestObject.transform.localPosition;

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

        NearestObject.transform.localPosition = NearestObjectOriginalPosition;

        NearestObject = null;
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
        if (isNavigating == false || canDetectRaycast == false || NearestObject == null || isInitializing)
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
        if(isNavigating == false || canDetectRaycast == false || isInitializing)
        {
            return;
        }

        Debug.Log("[Player] Pointer enter.");

        isPointerEnter = true;

        _CornerObjController.HightlightCornerObjects();

        NRInput.LaserVisualActive = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (NearestObject != null)
        {
            RestoreNearestObject();
        }

        if (isNavigating == false || canDetectRaycast == false || isInitializing)
        {
            return;
        }

        Debug.Log("[Player] Pointer exit.");

        isPointerEnter = false;

        _CornerObjController.RestoreCornerObjects();

        NRInput.LaserVisualActive = true;

        ResetAll();
    }
}
