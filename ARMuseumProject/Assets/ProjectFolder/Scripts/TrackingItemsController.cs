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
    public Material[] RealisticMaterials;
    public Material[] DefaultMaterials;

    private bool isPointerEnter = false;
    private GameObject NearestObject = null;
    private Vector3[] ObjectPositionArray;
    private int ObjectCount = 0;
    private bool isNavigating;

    void Start()
    {
        ObjectCount = DisplayItemsLayer.transform.childCount;
        ObjectPositionArray = new Vector3[ObjectCount];

        // 初始化展示节点中所有组件的位置，方便后续计算最近距离物体
        for (int i = 0; i < ObjectCount; i++)
        {
            GameObject child = DisplayItemsLayer.transform.GetChild(i).gameObject;

            ObjectPositionArray[i] = transform.InverseTransformPoint(child.transform.position);
        }

        StopTracking();
    }

    void Update()
    {
        if(isPointerEnter == true && ObjectPositionArray.Length > 0)
        {
            UpdateNearestObject();
        }
    }

    public void StartTracking()
    {
        _CornerObjController.ActiveCornerObjects();

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        isNavigating = true;
    }

    public void StopTracking()
    {
        _CornerObjController.InactiveCornerObjects();

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        isNavigating = false;
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
        int index = NearestObject.transform.GetSiblingIndex();

        NearestObject.GetComponent<Renderer>().material = DefaultMaterials[index];

        ResetAll();
    }

    private void ActiveNearestObject()
    {
        if(NearestObject == null)
        {
            return;
        }

        int index = NearestObject.transform.GetSiblingIndex();

        NearestObject.GetComponent<Renderer>().material = RealisticMaterials[index];
    }

    public void ResetAll()
    {
        int DisplayItemsCount = DisplayItemsLayer.transform.childCount;

        for (int i = 0; i < DisplayItemsCount; i++)
        {
            DisplayItemsLayer.transform.GetChild(i).GetComponent<Renderer>().material = DefaultMaterials[i];
        }

        NearestObject = null;
    }

    private GameObject FindNearestObject()
    {
        RaycastResult firstHit = _Raycaster.FirstRaycastResult();
        Vector3 hitPointPosition = transform.InverseTransformPoint(firstHit.worldPosition);
        float nearsetDistance = float.MaxValue;
        GameObject currNearestObject = null;

        for (int i = 0; i < ObjectCount; i++)
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
        if (isNavigating == false || NearestObject == null)
        {
            Debug.Log("[Player] No visuable object to active.");

            return;
        }

        Debug.Log("[Player] Pointer click, the nearest object is: " + NearestObject.name);

        _GrabbableItemsController.ActiveGrabbableItem(NearestObject);

        NearestObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(isNavigating == false)
        {
            return;
        }

        Debug.Log("[Player] Pointer enter.");

        isPointerEnter = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isNavigating == false)
        {
            return;
        }

        Debug.Log("[Player] Pointer exit.");

        isPointerEnter = false;

        ResetAll();
    }
}
